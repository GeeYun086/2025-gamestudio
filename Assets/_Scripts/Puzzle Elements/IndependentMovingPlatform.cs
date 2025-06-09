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
    public class IndependentMovingPlatform : MonoBehaviour
    {
        [SerializeField] Vector3 _targetPoint = new(0, 0, 0);
        [SerializeField] float _moveSpeed = 1f;

        [SerializeField] LayerMask _collisionLayers = ~0;
        const float ReachThreshold = 0.1f;

        Rigidbody _rigidbody;
        Vector3 _pathStartPoint;
        Vector3 _currentDestination;
        bool _isMovingTowardsPathEndPoint;

        Collider _triggerCollider;
        GameObject _solidSurfaceChild;

        void Awake()
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

        void CreateSolidSurfaceChild()
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

        void Start()
        {
            _pathStartPoint = transform.position;

            _currentDestination = _targetPoint;
            _isMovingTowardsPathEndPoint = true;
        }

        void FixedUpdate()
        {
            if (!enabled) return;

            _rigidbody.MovePosition(Vector3.MoveTowards(transform.position,
                _currentDestination, _moveSpeed * Time.fixedDeltaTime));
            if (Vector3.Distance(transform.position, _currentDestination) < ReachThreshold) SwitchDirection();
        }

        void OnTriggerEnter(Collider other)
        {
            if (_solidSurfaceChild && other.gameObject == _solidSurfaceChild) return;
            if (other.GetComponent<PlayerMovement>()) return;
            if (!IsLayerInMask(_collisionLayers, other.gameObject.layer)) return;

            SwitchDirection();
        }

        void SwitchDirection()
        {
            _isMovingTowardsPathEndPoint = !_isMovingTowardsPathEndPoint;
            _currentDestination = _isMovingTowardsPathEndPoint ? _targetPoint : _pathStartPoint;
        }

        static bool IsLayerInMask(LayerMask layerMask, int layer) => layerMask == (layerMask | (1 << layer));

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, _targetPoint);
            Gizmos.DrawWireSphere(transform.position, 0.2f);
            Gizmos.DrawSphere(_targetPoint, 0.2f);
        }
    }
}