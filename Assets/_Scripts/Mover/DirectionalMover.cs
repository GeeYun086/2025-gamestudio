using UnityEngine;

namespace GravityGame.Mover
{
    public class DirectionalMover : MonoBehaviour
    {
        [SerializeField] Vector3 moveDirection = Vector3.right;
        [SerializeField] float moveSpeed = 1f;
        [SerializeField] float moveDistance = 10f;

        Vector3 _startPosition;
        Vector3 _targetPosition;
        Vector3 _currentDestination;

        bool _isMovingAway;

        void Start()
        {
            _startPosition = transform.position;
            _targetPosition = _startPosition + moveDirection.normalized * moveDistance;

            _currentDestination = _targetPosition;
            _isMovingAway = true;
        }

        void Update()
        {
            float step = moveSpeed * Time.deltaTime;
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
            var startPos = Application.isPlaying ? _startPosition : transform.position;
            Gizmos.DrawLine(startPos, startPos + moveDirection.normalized * moveDistance);
        }
    }
}