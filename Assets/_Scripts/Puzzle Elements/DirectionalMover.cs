using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Moves the GameObject back and forth along a specified direction and distance
    /// at a defined speed. This movement is kinematic (directly manipulates the transform).
    /// </summary>
    public class DirectionalMover : MonoBehaviour
    {
        [SerializeField] Vector3 _moveDirection = Vector3.right;
        [SerializeField] float _moveSpeed = 1f;
        [SerializeField] float _moveDistance = 10f;

        Vector3 _startPosition;
        Vector3 _targetPosition;
        Vector3 _currentDestination;

        bool _isMovingAway;

        void Start()
        {
            _startPosition = transform.position;
            _targetPosition = _startPosition + _moveDirection.normalized * _moveDistance;

            _currentDestination = _targetPosition;
            _isMovingAway = true;
        }

        void Update()
        {
            float step = _moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _currentDestination, step);
            if (!(Vector3.Distance(transform.position, _currentDestination) < 0.01f)) return;
            if (_isMovingAway) {
                _currentDestination = _startPosition;
                _isMovingAway = false;
            } else {
                _currentDestination = _targetPosition;
                _isMovingAway = true;
            }
        }

        void OnDrawGizmosSelected()
        {
            var startPos = Application.isPlaying && _startPosition != Vector3.zero ? _startPosition : transform.position;
            var endPos = Application.isPlaying && _targetPosition != Vector3.zero ? _targetPosition : startPos + _moveDirection.normalized * _moveDistance;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawSphere(startPos, 0.1f);
            Gizmos.DrawSphere(endPos, 0.1f);
        }
    }
}