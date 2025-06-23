using UnityEngine;

namespace GravityGame
{
    // platform that moves when redstone powered, if not powered it returns to its start position
    public class RedstoneMovingPlatform : MovingPlatformBase
    {
        [SerializeField] bool _isPowered;
        const float ReachThreshold = 0.1f;

        public bool IsPowered
        {
            get => _isPowered;
            set => _isPowered = value;
        }

        void FixedUpdate()
        {
            Vector3 target = _isPowered ? EndPosition : StartPoint;
            Quaternion targetRot = _isPowered ? Quaternion.Euler(EndRotation) : StartRotation;

            if (Vector3.Distance(Rigidbody.position, target) < ReachThreshold)
            {
                Rigidbody.MovePosition(target);
                Rigidbody.MoveRotation(targetRot);
                Rigidbody.linearVelocity = Vector3.zero;
            }
            else
            {
                Rigidbody.linearVelocity = (target - Rigidbody.position).normalized * _speed;
                float t = Mathf.Clamp01(Vector3.Distance(_isPowered ? StartPoint : EndPosition, Rigidbody.position) / PathDistance);
                Quaternion from = _isPowered ? StartRotation : Quaternion.Euler(EndRotation);
                Quaternion to = _isPowered ? Quaternion.Euler(EndRotation) : StartRotation;
                Rigidbody.MoveRotation(Quaternion.Slerp(from, to, t));
            }
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
