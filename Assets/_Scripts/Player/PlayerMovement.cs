using System.Linq;
using GravityGame.Gravity;
using GravityGame.Puzzle_Elements;
using GravityGame.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GravityGame.Player
{
    /// <summary>
    ///     Basic player ground movement including walking and jumping
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Ground Movement")]
        public float MaxMoveSpeed = 8.0f;
        public float MoveAcceleration = 70.0f;
        public float GroundFriction = 8.0f;

        public float MaxStepHeight = 0.5f;
        public float MaxAirStepHeight = 0.1f;
        public float MaxSlopeAngle = 40.0f;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 4.0f;
        public float AirAcceleration = 30.0f;
        public float AirDrag = 0.1f;

        [Header("Jumping")]
        public float JumpHeight = 1.4f;
        public Timer JumpPreGroundedGraceTime = new(0.15f); // input buffer
        public Timer JumpPostGroundedGraceTime = new(0.15f); // coyote time
        public Timer JumpCooldown = new(0.2f);

        [Header("Input")]
        public InputActionReference MoveInput;
        public InputActionReference JumpInput;

        [Header("Debug")]
        public bool DebugStepDetection;

        Rigidbody _rigidbody;
        CapsuleCollider _collider;
        PlayerCarry _carry;
        Camera _camera;

        Vector3 _inputDirection;
        Vector3 _lastGroundVelocity;
        GroundInfo _ground;

        struct GroundInfo
        {
            public bool HasAnyGround;
            public bool HasStableGround;

            public RaycastHit Hit;
            public Vector3 Normal;
            public float Angle;
        }

        Vector3 Gravity => GetComponent<GravityModifier>().Gravity;

        void OnEnable()
        {
            _collider = GetComponent<CapsuleCollider>();
            _rigidbody = GetComponent<Rigidbody>();
            _camera = GetComponentInChildren<Camera>();
            _carry = GetComponent<PlayerCarry>();

            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            JumpInput.action.performed += _ => JumpPreGroundedGraceTime.Start();
        }

        void Update()
        {
            var input = MoveInput.action.ReadValue<Vector2>().normalized;
            var right = _camera.transform.right;
            var forward = Vector3.Cross(right, transform.up);
            _inputDirection = forward * input.y + right * input.x;
            _inputDirection = Vector3.ClampMagnitude(_inputDirection, 1f);
        }

        void FixedUpdate()
        {
            transform.up = -Gravity;
            _ground = CheckGround(transform.position);
            Move();
            TryStepUp();
        }

        void Move()
        {
            float deltaTime = Time.fixedDeltaTime;
            var velocity = _rigidbody.linearVelocity;
            var gravity = Gravity;

            // Get ground velocity (e.g. moving platform)
            var dynamicGround = _ground.Hit.rigidbody; // or null
            var groundVelocity = Vector3.zero;
            if (_ground.HasStableGround) {
                if (dynamicGround)
                    groundVelocity = dynamicGround.linearVelocity;
                else if (_ground.Hit.collider.gameObject.TryGetComponent<ConveyorBelt>(out var conveyorBelt))
                    groundVelocity = conveyorBelt.Velocity;
            }
            var groundVelocityDelta = groundVelocity - _lastGroundVelocity;

            const float platformStopThreshold = 1.0f;
            bool groundStoppedImmediately = Vector3.Dot(groundVelocityDelta, _lastGroundVelocity) < 0
                                            && groundVelocityDelta.magnitude > platformStopThreshold;
            _lastGroundVelocity = groundVelocity;

            // Get jump velocity
            bool jumped = TryJump();

            // Friction and Slopes
            if (_ground.HasAnyGround) {
                // Ground Friction
                var velocityRelativeToGround = velocity - groundVelocity;
                if (_inputDirection != Vector3.zero) {
                    var velocityInInputDir = Vector3.Dot(velocityRelativeToGround, _inputDirection.normalized);
                    if (velocityInInputDir > 0 && velocityInInputDir < MaxMoveSpeed) {
                        // No friction in input direction
                        velocityRelativeToGround -= velocityInInputDir * _inputDirection.normalized;
                    }
                }
                velocity -= Vector3.ProjectOnPlane(velocityRelativeToGround, _ground.Normal) * (GroundFriction * deltaTime);

                // On even ground or walkable slope
                if (_ground.HasStableGround) {
                    // Project walk movement on slope
                    _inputDirection = Vector3.ProjectOnPlane(_inputDirection, _ground.Normal);

                    var upVelocity = Vector3.Project(velocity, transform.up);
                    float upVelocityValue = Vector3.Dot(upVelocity, transform.up) > 0 ? upVelocity.magnitude : -upVelocity.magnitude;

                    // don't stick to cubes
                    if (dynamicGround && dynamicGround.TryGetComponent<Carryable>(out _)) {
                        gravity = -transform.up * 10f;
                    }
                    // Stick to slope
                    else if (_ground.Angle > 0.1f) {
                        if (_inputDirection == Vector3.zero) {
                            // stick to slope when standing still
                            gravity = -_ground.Normal * 50f;
                        } else if (upVelocityValue > 0f && !jumped) {
                            // stick to slope when walking up it
                            gravity = -_ground.Normal * 50f;
                        }
                    } else {
                        gravity = -_ground.Normal * 10f;
                    }
                }
                // On Steep Slope
                else {
                    var slopeUp = Vector3.ProjectOnPlane(transform.up, _ground.Normal);
                    // Eliminate velocity up slopes
                    var slopeUpVelocity = Vector3.Project(velocity, slopeUp);
                    if (Vector3.Dot(slopeUp, slopeUpVelocity) > 0) {
                        velocity -= slopeUpVelocity;
                    }
                    // Eliminate input up slopes
                    var slopeUpInput = Vector3.Project(_inputDirection, slopeUp);
                    if (Vector3.Dot(slopeUp, slopeUpInput) > 0) {
                        _inputDirection -= slopeUpInput;
                    }
                }

                // Add moving ground velocity, don't apply if sudden platform movement -> player should get launched off
                if (!groundStoppedImmediately) {
                    velocity += groundVelocityDelta;
                }
            } else {
                // Air Drag
                velocity -= Vector3.ProjectOnPlane(velocity, transform.up) * (AirDrag * deltaTime);
            }

            // Ground Acceleration
            if (_inputDirection != Vector3.zero) {
                var velocityRelativeToGround = velocity - groundVelocity;
                var velocityInInputDir = Vector3.Project(velocityRelativeToGround, _inputDirection);
                bool movingInOppositeDirection = Vector3.Dot(velocityInInputDir, _inputDirection) < 0;

                float moveSpeed = _ground.HasStableGround ? MaxMoveSpeed : MaxAirMoveSpeed;
                float acceleration = _ground.HasStableGround ? MoveAcceleration : AirAcceleration;

                bool hasNotReachedMaxSpeed = velocityInInputDir.magnitude < moveSpeed || movingInOppositeDirection;
                if (hasNotReachedMaxSpeed) {
                    var desiredInputVelocity = _inputDirection.normalized * moveSpeed;
                    var newVelocityInInputDir = Vector3.MoveTowards(velocityInInputDir, desiredInputVelocity, acceleration * deltaTime);
                    velocityRelativeToGround += newVelocityInInputDir - velocityInInputDir;
                    velocity = velocityRelativeToGround + groundVelocity;
                }
            }

            // Jump
            if (jumped) {
                // Jump only preserves velocity in inputDirection
                var jumpForward = Vector3.zero;
                if (_inputDirection != Vector3.zero) {
                    var velocityRelativeToGround = velocity - groundVelocity;
                    float velocityInInputDir = Vector3.Dot(velocityRelativeToGround, _inputDirection);
                    // if jump angle < 90
                    if (velocityInInputDir > 0) {
                        float jumpForwardVelocity = Mathf.Max(
                            velocityInInputDir, // Either preserve all speed in current input dir (possibly faster than MoveSpeed)
                            Mathf.Min(
                                velocityRelativeToGround.magnitude, MaxMoveSpeed
                            ) // or if jumping at a larger angle, the total velocity up to MaxMoveSpeed
                        );
                        jumpForward = _inputDirection * jumpForwardVelocity;
                        jumpForward = Vector3.ProjectOnPlane(jumpForward, transform.up); // no additional up on slops
                    }
                }

                // Jump up
                float jumpUpVelocity = Mathf.Sqrt(JumpHeight * 2f * Gravity.magnitude);
                var jumpUp = transform.up * jumpUpVelocity;

                // un-ground yourself
                _ground = default;

                // push ground down
                if (dynamicGround) {
                    // Note TG: calculate velocity assuming normal gravity magnitude. The actual jump velocity is way higher and would push objects too far, since player has naturally higher gravity
                    float pushDownVelocity = Mathf.Sqrt(JumpHeight * 2f * 9.8f);
                    var pushDown = -transform.up * pushDownVelocity;
                    dynamicGround.AddForceAtPosition(pushDown * _rigidbody.mass, transform.position, ForceMode.Impulse);

                    // Don't allow jump when standing on cube with low mass, that is not grounded (to prevent box double jump)
                    if (!IsDynamicGroundGrounded(dynamicGround, pushDown)) {
                        jumpUp *= Mathf.Clamp01(dynamicGround.mass / _rigidbody.mass);
                    }
                }

                // Overwrite velocity
                velocity = jumpUp + jumpForward + groundVelocity;
                // Debug.Log($"up: {jumpUp} fwd: {jumpForward} ground: {groundVelocity}");
            }

            // Apply Force
            _rigidbody.AddForce(velocity - _rigidbody.linearVelocity, ForceMode.VelocityChange);
            _rigidbody.AddForce(gravity, ForceMode.Acceleration);
        }

        bool IsDynamicGroundGrounded(Rigidbody dynamicGround, Vector3 pushDownVelocity)
        {
            if (!dynamicGround.TryGetComponent<Carryable>(out _) || dynamicGround.mass >= _rigidbody.mass) {
                return true;
            }
            
            const float timeToHitGround = 0.05f;
            // first test with sweep
            var endPos = dynamicGround.position + pushDownVelocity * timeToHitGround;
            if (dynamicGround.SweepTest(
                    pushDownVelocity.normalized, out var hit,
                    pushDownVelocity.magnitude * timeToHitGround, QueryTriggerInteraction.Ignore
                )) {
                Debug.Log($"Cube has ground (sweep): {hit.collider.name}");
                DebugDraw(true);
                return true;
            }

            // second test failsafe (if already inside collider, sweep will not detect it)
            const float overlapScale = 0.9f; // to prevent rounding errors
            var layer = ~LayerMask.GetMask("Player") & ~dynamicGround.excludeLayers;
            var results = new Collider[2];
            var size = Physics.OverlapBoxNonAlloc(
                endPos, dynamicGround.transform.lossyScale * (0.5f * overlapScale), results, dynamicGround.rotation, layer
            );

            foreach (var potentialGround in results.Take(size)) {
                if (potentialGround != dynamicGround.GetComponent<Collider>()) {
                    Debug.Log($"Cube has ground (overlap): {potentialGround}");
                    DebugDraw(true);
                    return true;
                }
            }
            DebugDraw(false);
            return false;
            void DebugDraw(bool grounded) => Utils.DebugDraw.DrawCube(endPos, 1.0f, grounded ? Color.green : Color.red, 1.0f);
        }

        void TryStepUp()
        {
            if (FindStep() is { HasStableGround: true } step) {
                if (DebugStepDetection) Debug.Log("Player Stepped!");
                var difference = step.Hit.point - transform.position;
                var up = Vector3.Project(difference, transform.up);
                // Move up
                _rigidbody.MovePosition(transform.position + up.normalized * (up.magnitude + 0.05f));

                // Note TG: I think this is no longer needed (?)
                // Add fwd speed so you have enough to climb stair
                // float climbStairFwdBoost = 1.0f;
                // var fwd = difference - up;
                // _rigidbody.AddForce(fwd.normalized * climbStairFwdBoost, ForceMode.VelocityChange);

                // eliminate downwards velocity
                var upVelocity = Vector3.Project(_rigidbody.linearVelocity, transform.up);
                var onlyUpVelocity = Vector3.Dot(upVelocity, transform.up) > 0 ? upVelocity : Vector3.zero;
                _rigidbody.AddForce(onlyUpVelocity - upVelocity, ForceMode.VelocityChange);
                _rigidbody.AddForce(transform.up * 1.0f, ForceMode.VelocityChange);
            }
        }

        bool TryJump()
        {
            if (_ground.HasStableGround) JumpPostGroundedGraceTime.Start();
            bool hasJumpInput = JumpPreGroundedGraceTime.IsActive;
            bool canJump = JumpPostGroundedGraceTime.IsActive && !JumpCooldown.IsActive;
            if (!hasJumpInput || !canJump) return false;
            JumpPostGroundedGraceTime.Stop();
            JumpPreGroundedGraceTime.Stop();
            JumpCooldown.Start();
            return true;
        }

        GroundInfo CheckGround(Vector3 position)
        {
            const float margin = 0.3f;
            const float groundDistance = margin + 0.10f;
            int layerMask = ~LayerMask.GetMask("Player");
            float radius = _collider.radius * 0.9f;
            var feetPosition = position + (radius + margin) * transform.up;
            float distance = groundDistance;
            var down = -transform.up;

            GroundInfo ground = default;
            var results = new RaycastHit[4];
            int numResults = Physics.SphereCastNonAlloc(feetPosition, radius, down, results, distance, layerMask, QueryTriggerInteraction.Ignore);
            foreach (var originalHit in results.Take(numResults).OrderBy(hit => hit.distance)) {
                if (_carry.CarriedObject?.Collider == originalHit.collider)
                    continue;
                var verifiedHit = originalHit;
                if (originalHit.point == Vector3.zero) {
                    // Hit point is zero, the sphere cast may have started inside an object
                    continue;
                }
                if (Physics.Raycast(
                        originalHit.point + margin * transform.up, down, out var hit, distance, layerMask, QueryTriggerInteraction.Ignore
                    )) {
                    verifiedHit = hit; // Note TG: recast, because spherecast sometimes does not get the actual ground normal for some reason 
                }
                var info = new GroundInfo();
                info.Hit = verifiedHit;
                info.Normal = verifiedHit.normal;
                info.HasAnyGround = true;
                info.Angle = Vector3.Angle(info.Normal, transform.up);
                info.HasStableGround = info.Angle <= MaxSlopeAngle;
                ground = info;
                break;
            }

            DebugDraw.DrawSphere(feetPosition, radius, ground.HasAnyGround ? Color.green : Color.red);
            Debug.DrawRay(feetPosition, down * distance);
            DebugDraw.DrawSphere(feetPosition + down * distance, radius, ground.HasAnyGround ? Color.green : Color.red);
            return ground;
        }

        GroundInfo FindStep()
        {
            GroundInfo noStep = default;
            if (_ground is { HasAnyGround: true, HasStableGround: false }) return noStep; // no stepping on steep slope
            if (!_ground.HasAnyGround && Vector3.Dot(_rigidbody.linearVelocity, transform.up) > 1.0f)
                return noStep; // no air stepping when velocity is too high
            if (_inputDirection == Vector3.zero) return noStep; // no unintended stepping

            const float minStepHeight = 0.05f;
            const float stepForward = 0.05f;
            int layerMask = ~LayerMask.GetMask("Player");
            var input = Vector3.ProjectOnPlane(_inputDirection, _ground.HasStableGround ? _ground.Normal : transform.up);
            float maxStepHeight = _ground.HasStableGround ? MaxStepHeight : MaxAirStepHeight;

            float distance = _collider.height;
            var rayFront = input.normalized * (_collider.radius + stepForward);
            var rayUp = (distance + minStepHeight) * transform.up;
            var origin = transform.position + rayUp + rayFront;
            var dir = -transform.up;

            if (!Physics.Raycast(origin, dir, out var hit, distance, layerMask)) {
                // no hit
                if (DebugStepDetection) Debug.DrawRay(origin, dir * distance, Color.red);
                return noStep;
            }
            float stepHeight = distance - hit.distance;

            bool inStepThreshold = stepHeight <= maxStepHeight;
            var stepGround = CheckGround(hit.point);
            if (inStepThreshold && stepGround.HasStableGround) {
                // Check if step is obstructed
                var start = stepGround.Hit.point + 0.05f * stepGround.Normal;
                var p1 = start + _collider.radius * transform.up;
                var p2 = start + (_collider.height - _collider.radius) * transform.up;
                var results = new Collider[1];
                int numResults = Physics.OverlapCapsuleNonAlloc(p1, p2, _collider.radius, results, layerMask, QueryTriggerInteraction.Ignore);
                if (DebugStepDetection) DebugDraw.DrawSphere(p1, _collider.radius, Color.black);
                if (DebugStepDetection) DebugDraw.DrawSphere(p2, _collider.radius, Color.black);
                if (numResults > 0) {
                    if (DebugStepDetection) Debug.DrawRay(origin, dir * hit.distance, Color.black);
                    if (DebugStepDetection) Debug.Log($"step obstructed by {results[0].name}");
                    return noStep;
                }

                // Good hit! We can step
                if (DebugStepDetection) Debug.DrawRay(origin, dir * hit.distance, Color.green);
                return stepGround;
            }

            // bad hit
            if (DebugStepDetection) Debug.DrawRay(origin, dir * hit.distance, Color.yellow);
            return noStep;
        }
    }
}