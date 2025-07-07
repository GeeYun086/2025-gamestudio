using System.Collections.Generic;
using GravityGame.Gravity;
using GravityGame.Puzzle_Elements;
using log4net.Appender;
using UnityEngine;
using UnityEngine.AI;

namespace GravityGame.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class SpiderCarrierWalker : MonoBehaviour
    {
        [SerializeField] Transform _waypointParent;
        
        [SerializeField] Transform _carrySocket;
        [SerializeField] float _detectionRadius = 2f;
        
        [SerializeField] float _rotationSpeed = 5f;

        [SerializeField] float _normalRayDistance = 1f;

        [SerializeField] LayerMask _surfaceMask = ~0;
        [SerializeField] LayerMask _carryableMask = ~0;
        [SerializeField] float _ignoreSeconds = 3f;
        Dictionary<GameObject, float> _ignoreUntil = new();
        GravityModifier _carriedGravity;
        FixedJoint _carryJoint;
        
        private NavMeshAgent _agent;
        SphereCollider _detectionTrigger;
        private List<Transform> _waypoints = new List<Transform>();
        private int _currentIndex = 0;
        private int _direction = 1; // +1 = forward, -1 = backward
        
        private Carryable _targetCarryable;
        private Carryable _carriedCarryable;
        bool _isApproachingObject = false;
        bool _isCarrying = false;
        float _originalStoppingDistance;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.updatePosition = true;
            _agent.updateRotation = false;
            
            _ignoreUntil.Clear();
            
            _detectionTrigger = GetComponent<SphereCollider>();
            _detectionTrigger.isTrigger = true;
            _detectionTrigger.radius = _detectionRadius;
            _detectionTrigger.center = Vector3.zero;
            
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
            if (_detectionTrigger == null)
                _detectionTrigger = GetComponent<SphereCollider>();
            if (_detectionTrigger != null)
                _detectionTrigger.radius = _detectionRadius;
        }

        void Start()
        {
            if (_waypoints.Count > 0)
                _agent.SetDestination(_waypoints[_currentIndex].position);
        }

        void Update()
        {
            if (_isCarrying && _carriedGravity != null && !IsDefaultGravity(_carriedGravity.GravityDirection))
                DropCarried();
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
            Transform objT = _targetCarryable.transform;
            objT.SetParent(_carrySocket, true);
            objT.localPosition = Vector3.zero;
            objT.localRotation = Quaternion.identity;
            objT.GetComponent<Rigidbody>().isKinematic = true;

            _carriedCarryable = _targetCarryable;
            _carriedGravity = _carriedCarryable.GetComponent<GravityModifier>();
            _isCarrying = true;
            _isApproachingObject = false;
            _targetCarryable = null;
            
            _agent.stoppingDistance = _originalStoppingDistance;
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
            if (Physics.Raycast(transform.position, -moveDir, out hit, _normalRayDistance, _surfaceMask))
                return hit.normal;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, _normalRayDistance, _surfaceMask))
                return hit.normal;
            return Vector3.up;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isCarrying || _isApproachingObject)
                return;
            if (((1 << other.gameObject.layer) & _carryableMask) == 0)
                return;
            GameObject go = other.gameObject;
            if (_ignoreUntil.TryGetValue(go, out float t) && Time.time < t)
                return;
            var carry = go.GetComponent<Carryable>();
            if (carry == null)
                return;
            var gm = go.GetComponent<GravityModifier>();
            if (gm != null && !IsDefaultGravity(gm.GravityDirection))
                return;
            _targetCarryable = carry;
            GetComponentInChildren<RiderAttach>().CanAttach = true;
            _isApproachingObject = true;
            _agent.stoppingDistance = 1f;
            _agent.SetDestination(carry.transform.position);
        }

        void DropCarried()
        {
            if (_carriedCarryable != null) {
                _carriedCarryable.transform.SetParent(null, true);
                Rigidbody rb = _carriedCarryable.GetComponent<Rigidbody>();
                if (rb != null) {
                    rb.isKinematic = false;
                }
                GameObject go = _carriedCarryable.gameObject;
                _ignoreUntil[go] = Time.time + _ignoreSeconds;
                
                _carriedCarryable = null;
                _isCarrying = false;
                _carriedGravity = null;
                
                GetComponentInChildren<RiderAttach>().CanAttach = false;
                
                _agent.stoppingDistance = _originalStoppingDistance;
                _agent.SetDestination(_waypoints[_currentIndex].position);
            }
        }

        public void ForceDropCarryable()
        {
            if (_isCarrying || _carriedCarryable != null)
                DropCarried();
        }
        
        bool IsDefaultGravity(Vector3 dir) => Vector3.Angle(dir.normalized, Vector3.down) < 1f;
    }
}
