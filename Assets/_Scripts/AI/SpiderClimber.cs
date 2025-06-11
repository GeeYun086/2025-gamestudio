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

        NavMeshAgent  _agent;
        NavMeshPatrol _patrol;
        bool          _busy;

        void Awake()
        {
            _agent  = GetComponent<NavMeshAgent>();
            _patrol = GetComponent<NavMeshPatrol>();
        }

        public bool TryHandle(Transform wp)
        {
            if (_busy) return true;

            NavMeshPath probe = new NavMeshPath();
            _agent.CalculatePath(wp.position, probe);
            if (probe.status == NavMeshPathStatus.PathComplete)
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

            _agent.isStopped = false;
            _agent.updatePosition = true;
            _agent.updateRotation = true;
            _agent.SetDestination(hit.position);

            while (_agent.pathPending ||
                   _agent.remainingDistance > _agent.stoppingDistance + 0.05f)
                yield return null;

            _agent.updatePosition = false;
            _agent.updateRotation = false;
            _agent.isStopped      = true;

            yield return new WaitForSeconds(_hangTime);

            Vector3 startPos     = transform.position;
            Quaternion startRot  = transform.rotation;
            Vector3 endPos       = wp.position;
            Quaternion endRot    = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(transform.forward, wp.up),
                wp.up);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / _climbTime;
                float k = _ease.Evaluate(t);
                transform.position = Vector3.Lerp(startPos, endPos, k);
                transform.rotation = Quaternion.Slerp(startRot, endRot, k);
                yield return null;
            }

            _agent.Warp(endPos);   
            _agent.updateUpAxis   = false; 
            _agent.updatePosition = true;
            _agent.updateRotation = true;
            _agent.isStopped      = false;

            _patrol.enabled = true;
            _busy = false;
        }

    }
}
