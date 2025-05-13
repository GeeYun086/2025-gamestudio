using GravityGame.Player;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Moves the GameObject back and forth between its initial start position and a target end point.
    /// Reverses direction upon colliding with specified layers (excluding the Player).
    /// Provides a solid surface for other physics objects (like players) to stand on.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class DirectionalMover : MonoBehaviour
    {
        [SerializeField] private Transform _targetPointTransform;
        [SerializeField] private Vector3 _targetPointManual = new(0, 0, 0);
        [SerializeField] private float _moveSpeed = 1f;

        [SerializeField] private LayerMask _collisionLayers = ~0;
        private const float ReachThreshold = 0.1f;

        private Rigidbody _rigidbody;
        private Vector3 _pathStartPoint;
        private Vector3 _pathEndPoint;
        private Vector3 _currentDestination;
        private bool _isMovingTowardsPathEndPoint;

        private Collider _triggerCollider;
        private GameObject _solidSurfaceChild;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            _triggerCollider = GetComponent<Collider>();
            if (!_triggerCollider)
            {
                enabled = false;
                return;
            }

            _triggerCollider.isTrigger = true;
            CreateSolidSurfaceChild();
        }

        private void CreateSolidSurfaceChild()
        {
            var existingChild = transform.Find(gameObject.name + "_SolidSurface");
            if (existingChild)
            {
                _solidSurfaceChild = existingChild.gameObject;
                return;
            }

            _solidSurfaceChild = new GameObject(gameObject.name + "_SolidSurface");
            _solidSurfaceChild.transform.SetParent(transform);
            _solidSurfaceChild.transform.localPosition = Vector3.zero;
            _solidSurfaceChild.transform.localRotation = Quaternion.identity;
            _solidSurfaceChild.transform.localScale = Vector3.one;
            _solidSurfaceChild.layer = gameObject.layer;

            switch (_triggerCollider)
            {
                case BoxCollider parentBox:
                {
                    var childBox = _solidSurfaceChild.AddComponent<BoxCollider>();
                    childBox.center = parentBox.center;
                    childBox.size = parentBox.size;
                    break;
                }
                case SphereCollider parentSphere:
                {
                    var childSphere = _solidSurfaceChild.AddComponent<SphereCollider>();
                    childSphere.center = parentSphere.center;
                    childSphere.radius = parentSphere.radius;
                    break;
                }
                case CapsuleCollider parentCapsule:
                {
                    var childCapsule = _solidSurfaceChild.AddComponent<CapsuleCollider>();
                    childCapsule.center = parentCapsule.center;
                    childCapsule.radius = parentCapsule.radius;
                    childCapsule.height = parentCapsule.height;
                    childCapsule.direction = parentCapsule.direction;
                    break;
                }
                case MeshCollider parentMesh:
                {
                    var childMesh = _solidSurfaceChild.AddComponent<MeshCollider>();
                    childMesh.sharedMesh = parentMesh.sharedMesh;
                    childMesh.convex = true;
                    break;
                }
                default:
                    Debug.LogWarning(
                        $"Collider type '{_triggerCollider.GetType()}' is not supported for solid surface creation",
                        this);
                    break;
            }
        }

        private void Start()
        {
            _pathStartPoint = transform.position;
            _pathEndPoint = _targetPointTransform ? _targetPointTransform.position : _targetPointManual;

            _currentDestination = _pathEndPoint;
            _isMovingTowardsPathEndPoint = true;
        }

        private void FixedUpdate()
        {
            if (!enabled) return;

            _rigidbody.MovePosition(Vector3.MoveTowards(transform.position,
                _currentDestination, _moveSpeed * Time.fixedDeltaTime));
            if (Vector3.Distance(transform.position, _currentDestination) < ReachThreshold) SwitchDirection();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_solidSurfaceChild && other.gameObject == _solidSurfaceChild) return;
            if (other.GetComponent<PlayerMovement>()) return;
            if (!IsLayerInMask(_collisionLayers, other.gameObject.layer)) return;

            SwitchDirection();
        }

        private void SwitchDirection()
        {
            _isMovingTowardsPathEndPoint = !_isMovingTowardsPathEndPoint;
            _currentDestination = _isMovingTowardsPathEndPoint ? _pathEndPoint : _pathStartPoint;
        }

        private static bool IsLayerInMask(LayerMask layerMask, int layer) => layerMask == (layerMask | (1 << layer));

        private void OnDrawGizmosSelected()
        {
            Vector3 gizmoPathStart, gizmoPathEnd;
            var currentPosition = transform.position;

            if (Application.isPlaying && enabled)
            {
                gizmoPathStart = _pathStartPoint;
                gizmoPathEnd = _pathEndPoint;
            }
            else
            {
                gizmoPathStart = currentPosition;
                gizmoPathEnd = _targetPointTransform ? _targetPointTransform.position : _targetPointManual;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(gizmoPathStart, gizmoPathEnd);
            Gizmos.DrawWireSphere(gizmoPathStart, 0.2f);
            Gizmos.DrawSphere(gizmoPathEnd, 0.2f);
        }
    }
}