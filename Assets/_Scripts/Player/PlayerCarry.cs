using System;
using System.Linq;
using GravityGame.Puzzle_Elements;
using GravityGame.Utils;
using UnityEditor;
using UnityEngine;

namespace GravityGame.Player
{
    /// <summary>
    ///     Manages the state of carrying a Carryable object.
    /// </summary>
    public class PlayerCarry : MonoBehaviour
    {
        public Transform MinCarryPoint;
        public Transform MaxCarryPoint;
        public float CarryDistance = 2f;
        public float MaxCarryDistance = 5f;
        public float MaxCarryMass = 250f;
        public float MaxVerticalCarryAngle = 60f;
        public float MinVerticalCarryAngle = -60f;
        public float CarrySpeed = 30f;
        public LayerMask ExcludeCarryObstaclesLayer;

        Carryable _currentlyCarrying;
        PlayerMovement _playerMovement;
        FirstPersonCameraController _camera;
        Collider[] _playerColliders;
        Vector3 _carryPosition;


        bool _isYPositionFrozen;
        float _yOffset;

        Vector3 MaxCarryPosition
        {
            get {
                var right = _camera.transform.right;
                var forward = Vector3.Cross(right, transform.up);
                var origin = _camera.transform.position;
                var lookDown = Mathf.Clamp(_camera.LookDownRotation, MinVerticalCarryAngle, MaxVerticalCarryAngle);
                var clampedLookDown = Quaternion.AngleAxis(lookDown, right);
                var carryDirection = clampedLookDown * forward;
                float carryDistance = MaxCarryPoint.localPosition.z;
                var pos = origin + carryDirection * carryDistance;
                return pos;
            }
        }
        Quaternion CarryRotation
        {
            get {
                var right = _camera.transform.right;
                var forward = Vector3.Cross(right, transform.up);
                return Quaternion.LookRotation(forward, transform.up);
            }
        }

        void OnEnable()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _camera = GetComponentInChildren<FirstPersonCameraController>();
            _playerColliders = GetComponentsInChildren<Collider>();
        }

        public bool IsCarrying => _currentlyCarrying;

        void Update()
        {
            UpdateCarryPosition(Time.deltaTime);

            if (IsCarrying) {
                if (Vector3.Distance(transform.position, _currentlyCarrying.transform.position) > MaxCarryDistance) {
                    AttemptRelease();
                    return;
                }

                if (_playerMovement.Ground.Hit.collider &&
                    _playerMovement.Ground.Hit.collider.gameObject == _currentlyCarrying.gameObject) {
                    AttemptRelease();
                }
            }
        }

        void UpdateCarryPosition(float deltaTime)
        {
            _carryPosition = FindCarryPoint();

            if (_currentlyCarrying == null) return;
            var rb = _currentlyCarrying.Rigidbody;
            var newPosition = Vector3.MoveTowards(rb.position, _carryPosition, CarrySpeed * deltaTime);
            rb.MovePosition(newPosition);
            rb.MoveRotation(CarryRotation);
            float drag = 1.0f;
            rb.linearVelocity *= 1f - drag * deltaTime;
            rb.angularVelocity *= 1f - drag * deltaTime;
        }

        Vector3 FindCarryPoint()
        {
            var farPos = MaxCarryPosition;
            var closePos = MinCarryPoint.position;
            var halfExtents = MinCarryPoint.localScale * 0.5f;
            var orientation = CarryRotation;
            
            { // check if min carry pos is inside wall
                var close = Physics.OverlapBox(closePos, halfExtents, orientation, ~ExcludeCarryObstaclesLayer);
                if(close.Length > 0) {
                    foreach (var c in close) {
                        Debug.Log($"too close {c.gameObject.name}");
                    }
                    return closePos;
                }
            }
            
            var direction = farPos - closePos;

            
            var hits = Physics.BoxCast(closePos, halfExtents, direction, out var hit, orientation, direction.magnitude, ~ExcludeCarryObstaclesLayer);
            if (!hits) {
                Debug.Log("far");
                return farPos;
            }
            Debug.Log($"something blocks: {hit.collider.gameObject.name}");
            return closePos + direction.normalized * hit.distance;
        }

        public void AttemptPickUp(Carryable carryable)
        {
            if (!IsCarrying && carryable && carryable.Rigidbody.mass <= MaxCarryMass) {
                _currentlyCarrying = carryable;
                _currentlyCarrying.PickUp();
                carryable.transform.parent = transform;
                // IgnorePlayerCollision(true);
            }
        }

        public void AttemptRelease()
        {
            if (IsCarrying) {
                _isYPositionFrozen = false;
                // IgnorePlayerCollision(false);
                _currentlyCarrying.Release();
                _currentlyCarrying.transform.parent = null;
                _currentlyCarrying = null;
            }
        }

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

        void OnDrawGizmos()
        {
            if (Application.isPlaying) {
                DrawGizmoCube(MinCarryPoint.position, CarryRotation, MinCarryPoint.localScale * 0.1f);
                DrawGizmoCube(MaxCarryPosition, CarryRotation, MinCarryPoint.localScale * 0.1f);
                Gizmos.DrawLine(MinCarryPoint.position, MaxCarryPosition);
                DrawGizmoCube(_carryPosition, CarryRotation, MinCarryPoint.localScale, false);
            }
        }

        // void LateUpdate()
        // {
        //     if (!IsCarrying) {
        //         _isYPositionFrozen = false;
        //         return;
        //     }
        //
        //     if (Mathf.Abs(_cameraController.LookDownRotation) > MaxVerticalRotation) {
        //         if (!_isYPositionFrozen) {
        //             _isYPositionFrozen = true;
        //             _yOffset = _carryPointTransform.position.y - _playerMovement.transform.position.y;
        //         }
        //
        //         var correctedPosition = _carryPointTransform.position;
        //         correctedPosition.y = _playerMovement.transform.position.y + _yOffset;
        //         _carryPointTransform.position = correctedPosition;
        //     } else {
        //         _isYPositionFrozen = false;
        //     }
        // }

        void IgnorePlayerCollision(bool ignore)
        {
            if (!_currentlyCarrying) return;
            var carriedCollider = _currentlyCarrying.GetComponent<Collider>();
            if (!carriedCollider) return;
            foreach (var playerCollider in _playerColliders) Physics.IgnoreCollision(playerCollider, carriedCollider, ignore);
        }
    }
}