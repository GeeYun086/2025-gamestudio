using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Moves a platform back and forth between its initial start position
    /// and a specified world target position using applied forces.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class IndependentMovingPlatform : MonoBehaviour
    {
        [SerializeField] Vector3 _endPosition = new(5, 0, 0);
        [SerializeField] float _force = 1000f;

        const float ReachThreshold = 0.2f;

        Rigidbody _rigidbody;
        Vector3 _startPoint;
        Vector3 _currentDestination;
        bool _isMovingToEnd;

        void Awake() => _rigidbody = GetComponent<Rigidbody>();

        void Start()
        {
            _startPoint = transform.position;
            _currentDestination = _endPosition;
            _isMovingToEnd = true;
        }

        void FixedUpdate()
        {
            if (Vector3.Distance(_rigidbody.position, _currentDestination) < ReachThreshold) {
                SwitchDirection();
            } else {
                _rigidbody.AddForce((_currentDestination - _rigidbody.position).normalized * _force);
            }
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