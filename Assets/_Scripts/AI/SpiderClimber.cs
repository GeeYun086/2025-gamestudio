using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace GravityGame.AI
{
    /// <summary>
    ///  Listens to NavMeshPatrol.GoTo(); if the target isn't on the NavMesh
    ///  it performs an animated climb based on the waypoint's transform.up.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class SpiderClimber : MonoBehaviour
    {
        [SerializeField] float _hangTime  = 0.35f;
        [SerializeField] float _climbTime = 1.25f;
        [SerializeField] AnimationCurve _ease = AnimationCurve.EaseInOut(0,0,1,1);
        [SerializeField] float _angleTolerance = 5f;
        [Header("Surface hugging")]
        [SerializeField] LayerMask _groundLayer = ~0;
        [SerializeField] float _arcAngle = 270f;
        [SerializeField] int _arcRes = 6;

        NavMeshAgent  _agent;
        NavMeshPatrol _patrol;
        bool          _busy;

        void Awake()
        {
            _agent  = GetComponent<NavMeshAgent>();
            _patrol = GetComponent<NavMeshPatrol>();
        }
        
        void LateUpdate()
        {
            float arcRadius = Mathf.Max(_agent.velocity.magnitude * Time.deltaTime, 0.05f);
            if (ArcCast(transform.position,
                    transform.rotation,
                    _arcAngle,
                    arcRadius,
                    _arcRes,
                    _groundLayer,
                    out RaycastHit hit))
            {
                transform.position = hit.point;
                transform.rotation =
                    Quaternion.FromToRotation(transform.up, hit.normal) *
                    transform.rotation;
            }
        }
        
        public static bool ArcCast(
            Vector3 center,
            Quaternion rotation,
            float angleDeg,
            float radius,
            int resolution,
            LayerMask layer,
            out RaycastHit hit
        )
        {
            rotation *= Quaternion.Euler(-angleDeg * 0.5f, 0, 0);
            for (int i = 0; i < resolution; i++) {
                Vector3 A = center + rotation * Vector3.forward * radius;
                rotation *= Quaternion.Euler(angleDeg / resolution, 0, 0);
                Vector3 B = center + rotation * Vector3.forward * radius;
                Vector3 AB = B - A;
                if (Physics.Raycast(A, AB, out hit, AB.magnitude * 1.001f, layer))
                    return true;
            }
            hit = default;
            return false;
        }

        public bool TryHandle(Transform wp)
        {
            Debug.Log("TryHandle");
            if (_busy) return true;

            NavMeshPath probe = new NavMeshPath();
            _agent.CalculatePath(wp.position, probe);
            
            Debug.Log(wp.rotation);
            
            bool orientationOK = Vector3.Angle(transform.up, wp.up) < _angleTolerance;
            
            if (probe.status == NavMeshPathStatus.PathComplete && orientationOK)
                return false;

            StartCoroutine(ClimbRoutine(wp));
            return true;
        }

        IEnumerator ClimbRoutine(Transform wp)
        {
            _busy = true;
            _patrol.enabled = false;

            float offset = 0.6f;
            Vector3 foot = wp.position - wp.up * offset;
            NavMesh.SamplePosition(foot, out var hit, 1f, NavMesh.AllAreas);

            

            _agent.SetDestination(hit.position);

            while (_agent.pathPending ||
                   _agent.remainingDistance > _agent.stoppingDistance + 0.05f)
                yield return null;
            
            _agent.isStopped      = true;

            yield return new WaitForSeconds(_hangTime);

            _agent.updateUpAxis   = false; 

            transform.SetPositionAndRotation(
                wp.position,
                Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, wp.up),
                    wp.up));
            _agent.nextPosition = transform.position;
            _agent.updateUpAxis = false;
            _agent.isStopped = false;
            _patrol.enabled = true;
            _busy = false;
        }

    }
}
