using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Moves (and rotates) a platform back and forth between its initial start position
    /// and a specified target position by controlling its velocity.
    /// /// The platform only moves when IsPowered is true.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class IndependentMovingPlatform : RedstoneComponent
    {
        [SerializeField] Vector3 _endPosition = new(5, 0, 0);
        [SerializeField] Vector3 _endRotation = new(0, 90, 0);
        [SerializeField] float _speed = 5f;

        const float ReachThreshold = 0.1f;

        Rigidbody _rigidbody;
        Vector3 _startPoint;
        Quaternion _startRotation;
        Vector3 _currentDestination;
        bool _isMovingToEnd;
        float _pathDistance;
        bool _isPowered;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        public override bool IsPowered
        {
            get => _isPowered;
            set {
                _isPowered = value;
                if (!_isPowered && _rigidbody) _rigidbody.linearVelocity = Vector3.zero;
            }
        }

        void Start()
        {
            _startPoint = _rigidbody.position;
            _startRotation = _rigidbody.rotation;
            _currentDestination = _endPosition;
            _isMovingToEnd = true;
            _pathDistance = Vector3.Distance(_startPoint, _endPosition);
        }

        void FixedUpdate()
        {
            if (!IsPowered) return;
            if (Vector3.Distance(_rigidbody.position, _currentDestination) < ReachThreshold) {
                _rigidbody.MovePosition(_currentDestination);
                _rigidbody.MoveRotation(_isMovingToEnd ? Quaternion.Euler(_endRotation) : _startRotation);
                SwitchDirection();
            } else {
                _rigidbody.linearVelocity = (_currentDestination - _rigidbody.position).normalized * _speed;
                UpdateRotation();
            }
        }

        void UpdateRotation()
        {
            float t = Mathf.Clamp01(Vector3.Distance(_isMovingToEnd ? _startPoint : _endPosition, _rigidbody.position) / _pathDistance);

            var sourceRotation = _isMovingToEnd ? _startRotation : Quaternion.Euler(_endRotation);
            var targetRotation = _isMovingToEnd ? Quaternion.Euler(_endRotation) : _startRotation;
            _rigidbody.MoveRotation(Quaternion.Slerp(sourceRotation, targetRotation, t));
        }

        void SwitchDirection()
        {
            _isMovingToEnd = !_isMovingToEnd;
            _currentDestination = _isMovingToEnd ? _endPosition : _startPoint;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, _endPosition);
            Gizmos.DrawSphere(_endPosition, 0.25f);
        }
    }
}