using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace GravityGame
{
    /// <summary>
    ///     Adds a "pick up and carry" layer on top of NavMeshPatrol.
    ///     The agent pauses its patrol when it sees a tagged object,
    ///     walks to it, and parents it to a CarrySocket.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class SpiderCarrier : MonoBehaviour
    {
        [Header("Pick-up timing & look")] public float waitBeforePickup = 0.35f;
        [SerializeField] private float moveTime = 0.25f;
        [SerializeField] private AnimationCurve heightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public string targetTag = "Carryable";
        public float lookAhead = 2.0f;
        public float detectionRadius = 2f;
        public float grabDistance = 1.5f;
        public Transform carrySocket;

        private NavMeshAgent _agent;
        private GameObject _currentTarget;
        private bool _foundObject;

        private float _originalStopping;
        private NavMeshPatrol _patrol;

        private State _state = State.Patrol;

        private void Update()
        {
            switch (_state)
            {
                case State.Patrol:
                    TickPatrol();
                    break;
                case State.Acquire:
                    TickAcquire();
                    break;
                case State.Carrying:
                    break;
            }
        }

        private void OnEnable()
        {
            _agent = GetComponent<NavMeshAgent>();
            _patrol = GetComponent<NavMeshPatrol>();

            if (carrySocket == null) Debug.LogError("SpiderCarrier needs a CarrySocket assigned!");
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying == false) return;

            Gizmos.color = Color.cyan;
            var origin = transform.position + Vector3.up * 0.2f;
            var dir = _agent != null && _agent.velocity.sqrMagnitude > 0.01f
                ? _agent.velocity.normalized
                : transform.forward;
            Gizmos.DrawLine(origin, origin + dir * lookAhead);
            Gizmos.DrawWireSphere(origin + dir * lookAhead, detectionRadius);
        }
#endif

        private void TickPatrol()
        {
            var origin = transform.position + Vector3.up * 0.2f;
            var dir = _agent.velocity.sqrMagnitude > 0.01f ? _agent.velocity.normalized : transform.forward;

            if (Physics.SphereCast(origin, detectionRadius, dir, out var hit, lookAhead))
                if (hit.collider.CompareTag(targetTag))
                {
                    _currentTarget = hit.collider.gameObject;
                    _patrol.enabled = false;
                    _state = State.Acquire;
                    _originalStopping = _agent.stoppingDistance;
                    _agent.stoppingDistance = grabDistance;
                    var approach = GetApproachPoint(_currentTarget.transform, grabDistance);
                    _agent.SetDestination(approach);
                }
        }

        private void TickAcquire()
        {
            if (_currentTarget == null)
            {
                ResumePatrol();
                return;
            }

            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.05f && !_foundObject)
            {
                _foundObject = true;
                StartCoroutine(DoPickup(_currentTarget));
            }
        }

        private Vector3 GetApproachPoint(Transform target, float offset)
        {
            var dir = target.position - transform.position;
            dir.y = 0f;
            dir = dir.normalized;

            var point = target.position - dir * offset;

            NavMesh.SamplePosition(point, out var hit, 1f, NavMesh.AllAreas);
            return hit.position;
        }

        private IEnumerator DoPickup(GameObject obj)
        {
            _agent.isStopped = true;
            yield return new WaitForSeconds(waitBeforePickup);
            if (obj.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            if (obj.TryGetComponent(out Collider col)) col.enabled = false;

            var startPos = obj.transform.position;
            var startRot = obj.transform.rotation;
            var endPos = carrySocket.position;
            var endRot = carrySocket.rotation;

            var t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / moveTime;
                var yArc = heightCurve.Evaluate(t) * 0.15f;
                obj.transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * yArc;
                obj.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
                yield return null;
            }

            obj.transform.SetParent(carrySocket, true);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            _agent.isStopped = false;
            _currentTarget = null;
            _state = State.Carrying;
            ResumePatrol();
        }

        private void ResumePatrol()
        {
            _patrol.enabled = true;
            _agent.stoppingDistance = _originalStopping;
            _patrol.RepathToCurrentTarget();
            _state = State.Patrol;
        }

        private enum State
        {
            Patrol,
            Acquire,
            Carrying
        }
    }
}