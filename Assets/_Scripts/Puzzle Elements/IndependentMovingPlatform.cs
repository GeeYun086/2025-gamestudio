using System;
using GravityGame.Gravity;
using GravityGame.SaveAndLoadSystem;
using GravityGame.Utils;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     Moves (and rotates) a platform back and forth between its initial start position
    ///     and a specified target position by controlling its velocity.
    ///     /// The platform only moves when IsPowered is true.
    /// </summary>
    public class IndependentMovingPlatform : RedstoneComponent, ISaveData<IndependentMovingPlatform.SaveData>
    {
        [SerializeField] Transform _startPoint;
        [SerializeField] Transform _endPoint;
        [SerializeField] Rigidbody _rigidbody;

        [SerializeField] float _speed = 5f;
        [SerializeField] Behavior _behavior;

        const float ReachThreshold = 0.01f;

        enum Behavior { Looping, WhenPoweredLooping, WhenPoweredMoveToB }

        bool _isPowered;
        bool _isMovingToEnd;

        void Awake()
        {
            _rigidbody.useGravity = false;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        public override bool IsPowered
        {
            get => _isPowered;
            set {
                _isPowered = value;
                if (_behavior is Behavior.WhenPoweredMoveToB)
                    _isMovingToEnd = _isPowered;
            }
        }

        void FixedUpdate()
        {
            var startPos = _startPoint.position;
            var startRot = _startPoint.rotation;
            var endPos = _endPoint.position;
            var endRot = _endPoint.rotation;

            var destination = _isMovingToEnd ? endPos : startPos;
            bool hasReachedDestination = Vector3.Distance(_rigidbody.position, destination) < ReachThreshold;
            if (hasReachedDestination) {
                _rigidbody.MovePosition(destination);
                _rigidbody.MoveRotation(_isMovingToEnd ? endRot : startRot);
                if (_behavior is Behavior.Looping or Behavior.WhenPoweredLooping) {
                    SwitchDirection();
                } else {
                    _rigidbody.linearVelocity = Vector3.zero;
                }
            } else if (_behavior == Behavior.WhenPoweredLooping && !IsPowered) {
                _rigidbody.linearVelocity = Vector3.zero;
            } else {
                // Update Position
                var newPosition = Vector3.MoveTowards(_rigidbody.position, destination, _speed * Time.fixedDeltaTime);
                _rigidbody.linearVelocity = (newPosition - _rigidbody.position) / Time.fixedDeltaTime;

                // Update Rotation
                float pathDistance = Vector3.Distance(startPos, endPos);
                float t = Mathf.Clamp01(Vector3.Distance(_isMovingToEnd ? startPos : endPos, _rigidbody.position) / pathDistance);
                var from = _isMovingToEnd ? startRot : endRot;
                var to = _isMovingToEnd ? endRot : startRot;
                _rigidbody.MoveRotation(Quaternion.Slerp(from, to, t));
            }
        }

        void SwitchDirection() => _isMovingToEnd = !_isMovingToEnd;

        void OnDrawGizmos()
        {
            if (_endPoint == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_startPoint.position, _endPoint.position);
            DebugDraw.DrawGizmoCube(_endPoint.position, _endPoint.rotation, _rigidbody.transform.lossyScale);
        }

    #region Save and Load

        [Serializable]
        public struct SaveData
        {
            public bool IsMovingToEnd;
            public Vector3 Position;
        }

        public SaveData Save() =>
            new() {
                IsMovingToEnd = _isMovingToEnd,
                Position = _rigidbody.position
            };

        public void Load(SaveData data)
        {
            _rigidbody.MovePosition(data.Position);
            _isMovingToEnd = data.IsMovingToEnd;
        }

        [field: SerializeField] public int SaveDataID { get; set; }

    #endregion
    }
}