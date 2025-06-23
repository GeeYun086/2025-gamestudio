using UnityEngine;

namespace GravityGame
{
    // baseclass for moving platforms
    [RequireComponent(typeof(Rigidbody))]
    public abstract class MovingPlatformBase : MonoBehaviour
    {
        [SerializeField] protected Transform EndPointTransform;
        [SerializeField] protected Vector3 EndRotation = new(0, 90, 0);
        [SerializeField] protected float _speed = 5f;

        protected Rigidbody Rigidbody;
        protected Vector3 StartPoint;
        protected Vector3 EndPosition;
        protected Quaternion StartRotation;
        protected float PathDistance;

        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.useGravity = false;
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        protected virtual void Start()
        {
            StartPoint = Rigidbody.position;
            StartRotation = Rigidbody.rotation;
            if (EndPointTransform != null)
                EndPosition = EndPointTransform.position;
            else
                EndPosition = StartPoint; // fallback
            PathDistance = Vector3.Distance(StartPoint, EndPosition);
        }
    }
}
