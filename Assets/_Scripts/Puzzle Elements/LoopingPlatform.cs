using UnityEngine;

namespace GravityGame
{
    // plattform that moves back and forth all the time
    public class LoopingPlatform : MovingPlatformBase
    {
        const float ReachThreshold = 0.1f;
        Vector3 _currentDestination;
        bool _isMovingToEnd;

        protected override void Start()
        {
            base.Start();
            _currentDestination = EndPosition;
            _isMovingToEnd = true;
        }

        void FixedUpdate()
        {
            if (Vector3.Distance(Rigidbody.position, _currentDestination) < ReachThreshold)
            {
                Rigidbody.MovePosition(_currentDestination);
                Rigidbody.MoveRotation(_isMovingToEnd ? Quaternion.Euler(EndRotation) : StartRotation);
                SwitchDirection();
            }
            else
            {
                Rigidbody.linearVelocity = (_currentDestination - Rigidbody.position).normalized * _speed;
                UpdateRotation();
            }
        }

        void UpdateRotation()
        {
            float t = Mathf.Clamp01(Vector3.Distance(_isMovingToEnd ? StartPoint : EndPosition, Rigidbody.position) / PathDistance);
            Quaternion from = _isMovingToEnd ? StartRotation : Quaternion.Euler(EndRotation);
            Quaternion to = _isMovingToEnd ? Quaternion.Euler(EndRotation) : StartRotation;
            Rigidbody.MoveRotation(Quaternion.Slerp(from, to, t));
        }

        void SwitchDirection()
        {
            _isMovingToEnd = !_isMovingToEnd;
            _currentDestination = _isMovingToEnd ? EndPosition : StartPoint;
        }
        void OnDrawGizmosSelected()
        {
            if (EndPointTransform == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, EndPointTransform.position);
            Gizmos.DrawSphere(EndPointTransform.position, 0.25f);
        }
    }
}
