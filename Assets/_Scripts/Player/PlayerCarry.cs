using System;
using System.Linq;
using GravityGame.Gravity;
using GravityGame.Puzzle_Elements;
using GravityGame.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace GravityGame.Player
{
    /// <summary>
    ///     Manages the state of carrying a Carryable object.
    /// </summary>
    public class PlayerCarry : MonoBehaviour
    {
        [Header("Carry Position Settings")]
        public Transform CarryPoint;
        public float MaxAngle = 60f;
        public float MinAngle = -30f;
        public float MinBackpackAngle = -52f;
        public float MinHeight = 0.55f;
        public float MaxHeight = 2.5f;
        public Transform BackpackCarryPoint;

        [Header("Carry Filter Settings")]
        public float MaxCarryDistance = 5f;
        public float MaxCarryMass = 250f;

        [Header("Other")]
        public float CarrySpeed = 30f;
        public float BackpackDelay = 0.5f;
        public CarryPhysicsState CarryPhysicsState = new() {
            PhysicsMaterial = null,
            Mass = 5,
            EnableCollider = false,
            EnableGravity = false,
        };

        PlayerMovement _playerMovement;
        FirstPersonCameraController _camera;
        Collider[] _playerColliders;
        
        Vector3 CarryBoxScale => CarryPoint.localScale;

        struct CarryInfo
        {
            [CanBeNull] public Carryable Object;

            public Vector3 Position;
            public Quaternion Rotation;
            public bool ShouldUseBackpack;
            public float LastUnobstructedTime;
            
            public CarryPhysicsState PreCarryPhysicsState;
        }

        CarryInfo _carry;

        void OnEnable()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _camera = GetComponentInChildren<FirstPersonCameraController>();
            _playerColliders = GetComponentsInChildren<Collider>();
        }

        public bool IsCarrying => _carry.Object != null;

        public void AttemptPickUp(Carryable obj)
        {
            if (_carry.Object) return;
            if (!obj || obj.Rigidbody.mass > MaxCarryMass) return;

            _carry.PreCarryPhysicsState = CarryPhysicsState.Get(obj);
            CarryPhysicsState.ApplyTo(obj);
            IgnorePlayerCollision(obj.gameObject, true);
            _carry.Object = obj;
        }

        public void AttemptRelease()
        {
            if (!_carry.Object) return;
            if (_carry.ShouldUseBackpack) return;
            var obj = _carry.Object;
            _carry.Object = null;
            _carry.PreCarryPhysicsState.ApplyTo(obj);
            IgnorePlayerCollision(obj.gameObject, false);
            obj.Rigidbody.linearVelocity = Vector3.zero;
            obj.Rigidbody.angularVelocity = Vector3.zero;

            obj.Collider.enabled = true;
        }

        void Update()
        {
            _carry.Position = GetCarryPosition();
            _carry.Rotation = GetCarryRotation();

            if (_carry.Object) {
                _carry.ShouldUseBackpack = ShouldUseBackpack();
                _carry.Object.Collider.enabled = !_carry.ShouldUseBackpack;

                if (_carry.ShouldUseBackpack) {
                    _carry.Position = BackpackCarryPoint.position;
                }

                if (Vector3.Distance(transform.position, _carry.Object.transform.position) > MaxCarryDistance) {
                    AttemptRelease();
                    return;
                }

                var ground = _playerMovement.Ground.Hit.collider;
                if (ground && ground == _carry.Object.Collider) {
                    AttemptRelease();
                }
            }
        }

        Vector3 GetCarryPosition()
        {
            // Clamp Min Max Carry Angle
            var right = _camera.transform.right;
            float clampedLookDownAngle = Mathf.Clamp(_camera.LookDownRotation, -MaxAngle, -MinAngle);
            var clampedLookDown = Quaternion.AngleAxis(clampedLookDownAngle, right);
            var lookRight = Quaternion.AngleAxis(_camera.LookRightRotation, transform.up);
            var localCarryPosition = clampedLookDown * lookRight * CarryPoint.localPosition;
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

        bool ShouldUseBackpack()
        {
            if (IsOverlappingWithPlayer(_carry.Position)) {
                return true;
            }
            if (FindObstructedCarryPosition() is { } pos && IsOverlappingWithPlayer(pos)) {
                return true;
            }
            if (-_camera.LookDownRotation < MinBackpackAngle) {
                return true;
            }
            return false;

            bool IsOverlappingWithPlayer(Vector3 position)
            {
                var halfExtents = CarryBoxScale * 0.5f;
                int layerMask = LayerMask.GetMask("Player");
                var results = new Collider[1];
                int overlappingObjects = Physics.OverlapBoxNonAlloc(position, halfExtents, results, _carry.Rotation, layerMask);
                return overlappingObjects > 0;
            }

            Vector3? FindObstructedCarryPosition()
            {
                if (_carry.Object == null) return null;
                var rb = _carry.Object.Rigidbody;
                var start = _camera.transform.position;
                var direction = _carry.Position - start;
                int layerMask = ~LayerMask.GetMask("Player");
                // if (rb.SweepTest(direction, out var hit, direction.magnitude)) {
                var halfExtents = CarryBoxScale * 0.5f;

                var results = new RaycastHit[10];
                int hitCount = Physics.BoxCastNonAlloc(
                    start, halfExtents, direction.normalized, results, _carry.Rotation, direction.magnitude, layerMask
                );
                foreach (var hit in results.Take(hitCount)) {
                    if (hit.collider == _carry.Object.Collider) continue;
                    if (hit.collider.enabled == false || hit.collider.isTrigger) continue;
                    var hitPos = start + direction.normalized * hit.distance;
                    DebugDraw.DrawCube(hitPos, 1f);
                    Debug.DrawRay(start, direction, Color.yellow);
                    return hitPos;
                }
                Debug.DrawRay(start, direction, Color.blue);
                return null;
            }
        }

        void FixedUpdate()
        {
            MoveCarriedObject(Time.fixedDeltaTime);
            RotateCarriedObject(Time.fixedDeltaTime);
            return;

            void MoveCarriedObject(float deltaTime)
            {
                if (_carry.Object == null) return;
                var rb = _carry.Object.Rigidbody;
                var newPosition = Vector3.MoveTowards(rb.position, _carry.Position, CarrySpeed * deltaTime);
                var velocity = (newPosition - rb.position) / deltaTime;
                rb.linearVelocity = velocity;
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
                if (Mathf.Abs(angleInRadians) > 0.001f) {
                    // Smoothly rotate toward the target at a constant angular speed
                    float angularSpeed = CarrySpeed;
                    float rotationThisFrame = Mathf.Min(angleInRadians, angularSpeed * deltaTime);
                    var angularVelocity = axis * rotationThisFrame / deltaTime;
                    rb.angularVelocity = angularVelocity;
                } else {
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        void IgnorePlayerCollision(GameObject obj, bool ignore)
        {
            if (!obj.TryGetComponent<Collider>(out var objCollider)) return;
            foreach (var playerCollider in _playerColliders) Physics.IgnoreCollision(playerCollider, objCollider, ignore);
        }

        void OnDrawGizmos()
        {
            if (Application.isPlaying) {
                DrawGizmoCube(_carry.Position, _carry.Rotation, CarryBoxScale);
            }
            return;

            void DrawGizmoCube(Vector3 position, Quaternion rotation, Vector3 scale, bool filled = false)
            {
                Gizmos.matrix = Matrix4x4.TRS(position, rotation, scale);
                if (filled) {
                    Gizmos.DrawCube(Vector3.zero, Vector3.one);
                } else {
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                }
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }

    [Serializable]
    public struct CarryPhysicsState
    {
        public PhysicsMaterial PhysicsMaterial;
        public float Mass;
        public bool EnableCollider;
        public bool EnableGravity;

        public static CarryPhysicsState Get(Carryable carryable) =>
            new() {
                PhysicsMaterial = carryable.Collider.material,
                Mass = carryable.Rigidbody.mass,
                EnableGravity = true,
                EnableCollider = true,
            };

        public void ApplyTo(Carryable carryable)
        {
            carryable.Collider.material = PhysicsMaterial;
            carryable.Rigidbody.mass = Mass;
            carryable.Collider.enabled = EnableCollider;

            if (carryable.TryGetComponent(out GravityModifier gravityModifier)) gravityModifier.enabled = EnableGravity;
            else carryable.Rigidbody.useGravity = EnableGravity;
        }
    }
}