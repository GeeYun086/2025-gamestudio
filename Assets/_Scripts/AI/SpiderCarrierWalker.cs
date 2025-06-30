using System.Collections.Generic;
using GravityGame.Puzzle_Elements;
using log4net.Appender;
using UnityEngine;
using UnityEngine.AI;

namespace GravityGame.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class SpiderCarrierWalker : MonoBehaviour
    {
        [Tooltip("Set your floor & wall waypoints here in order.")]
        [SerializeField] Transform _waypointParent;
        
        [SerializeField] Transform _carrySocket;
        [SerializeField] float _detectionRadius = 2f;
        
        [Tooltip("Speed at which the spider rotates to face its next move.")]
        [SerializeField] float _rotationSpeed = 5f;

        [Tooltip("Maximum distance to raycast for detecting surface normals.")]
        [SerializeField] float _normalRayDistance = 1f;

        [Tooltip("Layers that define walkable surfaces (floor + walls).")]
        [SerializeField] LayerMask _surfaceMask = ~0;
        [SerializeField] LayerMask _carryableMask = ~0;

        private NavMeshAgent _agent;
        SphereCollider _detectionCollider;
        private List<Transform> _waypoints = new List<Transform>();
        private int _currentIndex = 0;
        private int _direction = 1; // +1 = forward, -1 = backward
        
        private Carryable _targetCarryable;
        bool _isApproachingObject = false;
        bool _isCarrying = false;
        float _originalStoppingDistance;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.updatePosition = true;
            _agent.updateRotation = false;
            
            _detectionCollider = GetComponent<SphereCollider>();
            _detectionCollider.isTrigger = true;
            _detectionCollider.radius = _detectionRadius;
            _detectionCollider.center = Vector3.zero;
            
            _originalStoppingDistance = _agent.stoppingDistance;

            if (_waypointParent != null) {
                for (int i = 0; i < _waypointParent.childCount; i++) {
                    _waypoints.Add(_waypointParent.GetChild(i));
                }
            } else {
                Debug.LogWarning("Waypoint parent not assigned");
            }
        }

        void OnValidate()
        {
            if (_detectionCollider == null)
                _detectionCollider = GetComponent<SphereCollider>();
            if (_detectionCollider != null)
                _detectionCollider.radius = _detectionRadius;
        }

        void Start()
        {
            if (_waypoints.Count > 0)
                _agent.SetDestination(_waypoints[_currentIndex].position);
        }

        void Update()
        {
            if (_isApproachingObject && _targetCarryable != null) {
                if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance) {
                    PickUp();
                }
            } else {
                if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                    AdvanceWaypoint();
            }
            Vector3 desiredVel = _agent.desiredVelocity;
            if (desiredVel.sqrMagnitude > 0.01f)
            {
                Vector3 surfaceNormal = SampleSurfaceNormal(desiredVel.normalized);
                Quaternion targetRot = Quaternion.LookRotation(desiredVel, surfaceNormal);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                    _rotationSpeed * Time.deltaTime);
            }
        }

        private void PickUp()
        {
            _targetCarryable.transform.SetParent(_carrySocket);
            _targetCarryable.transform.localPosition = Vector3.zero;
            _targetCarryable.transform.localRotation = Quaternion.identity;
            _targetCarryable.GetComponent<Rigidbody>().isKinematic = true;
            _isCarrying = true;
            _isApproachingObject = false;
            _agent.stoppingDistance = _originalStoppingDistance;
            _targetCarryable = null;
            
            _agent.SetDestination(_waypoints[_currentIndex].position);
        }
        
        private void AdvanceWaypoint()
        {
            _currentIndex += _direction;
            if (_currentIndex >= _waypoints.Count)
            {
                _currentIndex = _waypoints.Count - 1;
                _direction = -1;
            }
            else if (_currentIndex < 0)
            {
                _currentIndex = 0;
                _direction = 1;
            }
            _agent.stoppingDistance = _originalStoppingDistance;
            _agent.SetDestination(_waypoints[_currentIndex].position);
        }
        
        private Vector3 SampleSurfaceNormal(Vector3 moveDir)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -moveDir, out hit,
                    _normalRayDistance, _surfaceMask))
            {
                return hit.normal;
            }
            if (Physics.Raycast(transform.position, Vector3.down, out hit,
                    _normalRayDistance, _surfaceMask))
            {
                return hit.normal;
            }
            return Vector3.up;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isCarrying && !_isApproachingObject) {
                if (((1 << other.gameObject.layer) & _carryableMask) != 0) {
                    Carryable carry = other.gameObject.GetComponent<Carryable>();
                    if (carry != null) {
                        _targetCarryable = carry;
                        _isApproachingObject = true;
                        //Vector3 dir = (carry.transform.position - transform.position).normalized;
                        //Vector3 approachPos = carry.transform.position - dir * _agent.stoppingDistance;
                        _agent.stoppingDistance = 1f;
                        _agent.SetDestination(carry.transform.position);
                    }
                }
            }
        }
    }
}
