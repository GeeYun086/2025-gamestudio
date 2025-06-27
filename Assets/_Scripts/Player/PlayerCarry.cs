using System;
using System.Linq;
using GravityGame.Gravity;
using GravityGame.Puzzle_Elements;
using GravityGame.SaveAndLoadSystem;
using GravityGame.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace GravityGame.Player
{
    /// <summary>
    ///     Manages the state of carrying a Carryable object.
    /// </summary>
    public class PlayerCarry : MonoBehaviour, ISaveData
    {
        [Header("Carry Position Settings")]
        public Transform CarryPoint;
        public Transform BackpackCarryPoint;

        public float MaxAngle = 60f;
        public float MinAngle = -30f;
        public float MinBackpackAngle = -52f;

        public float MinHeight = 0.55f;
        public float MaxHeight = 2.5f;

        public float MoveSpeed = 10f;
        public float RotationSpeed = 3f;

        [Header("Carry Physics")]
        public CarryPhysicsState CarryPhysicsState = new() {
            PhysicsMaterial = null,
            Mass = 5,
            EnableGravity = false
        };

        [Header("Other")]
        public Timer BackpackDelay = new(0.5f);

        [Tooltip("Carry Threshold")]
        public float MaxCarryMass = 250f;
        [Tooltip("Max distance before carry disconnects")]
        public float MaxCarryDistance = 5f;

        [Header("Audio")]
        public AudioClip CannotReleaseCarrySound;

        /// Used for box casts. Should be the (absolute) scale of the carried cube
        /// Note TG: We currently rely on the carried object being a cube mesh of size (1,1,1), which can then be scaled in unity
        Vector3 CarryBoxScale => _carry.Object ? _carry.Object.transform.lossyScale : Vector3.one;

        /* ---------------------------- settings end ----------------------------- */

        struct CarryInfo
        {
            [CanBeNull] public Carryable Object;

            public Vector3 Position;
            public Quaternion Rotation;

            public bool ShouldUseBackpack;
            public bool UsingBackpack;
            // (literal) edge case (at edges) where box sweep does not collide will wall
            public bool IsOverlappingWithPlayer;

            public CarryPhysicsState PreCarryPhysicsState;
        }

        Rigidbody _rigidbody;
        FirstPersonCameraController _camera;
        Collider[] _playerColliders;

        CarryInfo _carry; // All information on the current carry operation

        public Carryable CarriedObject => _carry.Object;

        void OnEnable()
        {
            _camera = GetComponentInChildren<FirstPersonCameraController>();
            _playerColliders = GetComponentsInChildren<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        public bool AttemptPickUp(Carryable obj)
        {
            if (_carry.Object) return false;
            if (!obj || obj.Rigidbody.mass > MaxCarryMass) return false;

            _carry.UsingBackpack = false;
            _carry.Object = obj;
            _carry.PreCarryPhysicsState = CarryPhysicsState.Get(obj);
            CarryPhysicsState.ApplyTo(obj);
            IgnorePlayerCollision(obj, true);
            return true;
        }

        public bool AttemptRelease(bool isFirstAttempt = true)
        {
            if (!_carry.Object) return false;
            if (_carry.ShouldUseBackpack || _carry.UsingBackpack || _carry.IsOverlappingWithPlayer) {
                if(isFirstAttempt)
                    GetComponent<AudioSource>().PlayOneShot(CannotReleaseCarrySound, 0.2f);
                return false;
            }
            ForceDrop();
            return true;
        }

        public void ForceDrop(Vector3 fallbackPosition = default)
        {
            if (!_carry.Object) return;
            var obj = _carry.Object;
            _carry.Object = null;
            _carry.PreCarryPhysicsState.ApplyTo(obj);
            obj.Rigidbody.linearVelocity = Vector3.zero;
            obj.Rigidbody.angularVelocity = Vector3.zero;
            obj.Collider.enabled = true;
            IgnorePlayerCollision(obj, false);
            if (_carry.UsingBackpack && fallbackPosition != default) {
                obj.Rigidbody.MovePosition(fallbackPosition);
            }
        }

        void Update()
        {
            _carry.Position = GetCarryPosition();
            _carry.Rotation = GetCarryRotation();

            if (_carry.Object) {
                UpdateBackpackState();
                if (IsReleaseNecessary()) AttemptRelease();
            }
        }

        bool IsReleaseNecessary()
        {
            if (!_carry.Object) return false;
            // Too far away
            if (Vector3.Distance(transform.position, _carry.Object.transform.position) > MaxCarryDistance) {
                return true;
            }
            return false;
        }

        Vector3 GetCarryPosition()
        {
            // Clamp Min Max Carry Angle
            var right = _camera.transform.right;
            float clampedLookDownAngle = Mathf.Clamp(_camera.LookDownRotation, -MaxAngle, -MinAngle);
            var clampedLookDown = Quaternion.AngleAxis(clampedLookDownAngle, right);
            var lookRight = Quaternion.AngleAxis(_camera.LookRightRotation, transform.up);
            var rotation = clampedLookDown * lookRight * Quaternion.FromToRotation(Vector3.up, transform.up);
            var localCarryPosition = rotation * CarryPoint.localPosition;
            var carryPosition = _camera.transform.position + localCarryPosition;
            // Clamp Min Max Carry Height
            var playerToCarryPos = carryPosition - transform.position;
            float carryUp = Vector3.Dot(playerToCarryPos, transform.up);
            float clampedCarryUp = Mathf.Clamp(carryUp, MinHeight, MaxHeight);
            playerToCarryPos += transform.up * (clampedCarryUp - carryUp);
            carryPosition = playerToCarryPos + transform.position;

            return carryPosition;
        }

        Quaternion GetCarryRotation()
        {
            var right = _camera.transform.right;
            var forward = Vector3.Cross(right, transform.up);
            return Quaternion.LookRotation(forward, transform.up);
        }

        void UpdateBackpackState()
        {
            if (!_carry.Object) return;
            bool hadUsedBackpack = _carry.ShouldUseBackpack;
            _carry.ShouldUseBackpack = ShouldUseBackpack();
            if (_carry.UsingBackpack != _carry.ShouldUseBackpack) {
                if (BackpackDelay.IsActive) {
                    return; // waiting
                }
                if (hadUsedBackpack != _carry.ShouldUseBackpack) {
                    BackpackDelay.Start(); // just switched -> start timer
                }
                if (!BackpackDelay.IsActive) {
                    _carry.UsingBackpack = _carry.ShouldUseBackpack; // timer over -> switch
                }
            }

            // Match backpack state
            _carry.Object.Collider.enabled = !_carry.UsingBackpack;
            if (_carry.UsingBackpack) _carry.Position = BackpackCarryPoint.position;
        }

        bool ShouldUseBackpack()
        {
            if (!_carry.Object) return false;
            _carry.IsOverlappingWithPlayer = IsOverlappingWithPlayer(_carry.Object.Rigidbody.position, _carry.Object.Rigidbody.rotation);

            // unobstructed look directions that get you into backpack mode (e.g. looking down)
            if (-_camera.LookDownRotation < MinBackpackAngle || IsOverlappingWithPlayer(_carry.Position, _carry.Rotation))
                return true;

            var takeOutOfBackpackPos = FindObstructedCarryPosition();
            bool cannotTakeOutOfBackpack = takeOutOfBackpackPos is { } pos && IsOverlappingWithPlayer(pos, _carry.Rotation);
            if (cannotTakeOutOfBackpack) {
                if (_carry.UsingBackpack) return true;
                if (_carry.IsOverlappingWithPlayer) return true;
            }

            return false;

            bool IsOverlappingWithPlayer(Vector3 position, Quaternion rotation)
            {
                var halfExtents = CarryBoxScale * 0.5f;
                int layerMask = LayerMask.GetMask("Player");
                var results = new Collider[1];
                int overlappingObjects = Physics.OverlapBoxNonAlloc(position, halfExtents, results, rotation, layerMask);
                return overlappingObjects > 0;
            }

            Vector3? FindObstructedCarryPosition()
            {
                if (_carry.Object == null) return null;
                var start = _camera.transform.position;
                var direction = _carry.Position - start;
                int layerMask = ~LayerMask.GetMask("Player", "Laser");
                const float overlapScale = 0.6f; // allow box to be inside player a little
                var halfExtents = CarryBoxScale * (0.5f * overlapScale);

                var results = new RaycastHit[10];
                int hitCount = Physics.BoxCastNonAlloc(
                    start, halfExtents, direction.normalized, results, _carry.Rotation, direction.magnitude, layerMask, QueryTriggerInteraction.Ignore
                );
                foreach (var hit in results.Take(hitCount)) {
                    if (hit.collider == _carry.Object.Collider) continue;
                    if (hit.collider.enabled == false) continue;
                    var hitPos = start + direction.normalized * hit.distance;
                    DebugDraw.DrawCube(hitPos, 1f);
                    Debug.DrawRay(start, direction, Color.yellow);
                    return hitPos;
                }
                Debug.DrawRay(start, direction, Color.blue);
                return null;
            }
        }

        Vector3 _lastLinearVelocity;

        void FixedUpdate()
        {
            MoveCarriedObject(Time.fixedDeltaTime);
            RotateCarriedObject(Time.fixedDeltaTime);
            return;

            void MoveCarriedObject(float deltaTime)
            {
                if (_carry.Object == null) return;
                var rb = _carry.Object.Rigidbody;
                var newPosition = Vector3.MoveTowards(rb.position, _carry.Position, MoveSpeed * deltaTime);
                var direction = newPosition - rb.position;
                var velocity = direction / deltaTime;
                if (IsOverlappingWithSomething(newPosition, rb.rotation, 0.95f) && !_carry.UsingBackpack) {
                    var delta = rb.linearVelocity - _lastLinearVelocity;
                    var otherVelocity = Vector3.MoveTowards(delta, Vector3.zero, 10f / Time.fixedDeltaTime);
                    velocity = Vector3.ClampMagnitude(velocity, 5f); // avoid cramming box into wall with too much speed
                    velocity += otherVelocity;
                }
                rb.linearVelocity = velocity;
                _lastLinearVelocity = rb.linearVelocity;
            }

            void RotateCarriedObject(float deltaTime)
            {
                if (_carry.Object == null) return;
                var rb = _carry.Object.Rigidbody;
                var deltaRotation = _carry.Rotation * Quaternion.Inverse(rb.rotation);
                deltaRotation.ToAngleAxis(out float angleInDegrees, out var axis);

                if (angleInDegrees > 180f) angleInDegrees -= 360f;

                // Convert to radians and normalize axis
                float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
                axis.Normalize();

                // Only apply if rotation is meaningful
                if (Mathf.Abs(angleInRadians) > 0.001f && !IsOverlappingWithSomething(rb.position, rb.rotation, 1.05f)) {
                    // Smoothly rotate toward the target at a constant angular speed
                    float angularSpeed = RotationSpeed;
                    float rotationThisFrame = Mathf.Min(angleInRadians, angularSpeed * deltaTime);
                    var angularVelocity = axis * rotationThisFrame / deltaTime;
                    rb.angularVelocity = angularVelocity;
                } else {
                    rb.angularVelocity = Vector3.zero;
                }
            }

            Collider IsOverlappingWithSomething(Vector3 position, Quaternion rotation, float scale = 1.0f)
            {
                var halfExtents = CarryBoxScale * (0.5f * scale);
                int layerMask = ~LayerMask.GetMask("Player", "Laser");
                var results = new Collider[2];
                int overlappingObjects = Physics.OverlapBoxNonAlloc(position, halfExtents, results, rotation, layerMask);
                var coll = results.Take(overlappingObjects).FirstOrDefault(b => b != _carry.Object?.Collider);
                // DebugDraw.DrawCube(position, scale, coll ? Color.green : Color.red);
                return coll;
            }
        }

        void IgnorePlayerCollision(Carryable obj, bool ignore)
        {
            foreach (var playerCollider in _playerColliders) Physics.IgnoreCollision(playerCollider, obj.Collider, ignore);
        }

        void OnDrawGizmos()
        {
            if (Application.isPlaying) {
                DebugDraw.DrawGizmoCube(_carry.Position, _carry.Rotation, CarryBoxScale);
            }
        }

    #region Save and Load

        public string SaveToJson() => "";

        public void LoadFromJson(string _)
        {
            if (_carry.Object) ForceDrop(_carry.Object.transform.position); // ensure restoring the physics state
        }

        [field: SerializeField] public int SaveDataID { get; set; }

    #endregion
    }

    [Serializable]
    public struct CarryPhysicsState
    {
        public PhysicsMaterial PhysicsMaterial;
        public float Mass;
        public bool EnableGravity;

        public static CarryPhysicsState Get(Carryable carryable) =>
            new() {
                PhysicsMaterial = carryable.Collider.sharedMaterial,
                Mass = carryable.Rigidbody.mass,
                EnableGravity = true
            };

        public void ApplyTo(Carryable carryable)
        {
            carryable.Collider.sharedMaterial = PhysicsMaterial;
            carryable.Rigidbody.mass = Mass;

            if (carryable.TryGetComponent(out GravityModifier gravityModifier)) gravityModifier.enabled = EnableGravity;
            else carryable.Rigidbody.useGravity = EnableGravity;
        }
    }
}