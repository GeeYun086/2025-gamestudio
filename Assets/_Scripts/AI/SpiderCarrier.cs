using System;
using System.Collections;
using GravityGame.AI;
using GravityGame.Puzzle_Elements;
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
        [Header("Pick-up timing & look")] 
        [SerializeField] float _waitBeforePickup = 0.35f;
        [SerializeField] float _moveTime = 0.25f;
        [SerializeField] AnimationCurve _heightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] float _lookAhead = 2.0f;
        [SerializeField] float _detectionRadius = 2f;
        [SerializeField] float _grabDistance = 1.5f;
        [SerializeField] Transform _carrySocket;

        NavMeshAgent _agent;
        GameObject _currentTarget;
        bool _foundObject;

        float _originalStopping;
        NavMeshPatrol _patrol;

        State _state = State.Patrol;

        void Start()
        {
            _agent.updateUpAxis = false;
        }

        void Update()
        {
            switch (_state) {
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

        void OnEnable()
        {
            _agent = GetComponent<NavMeshAgent>();
            _patrol = GetComponent<NavMeshPatrol>();

            if (_carrySocket == null) Debug.LogError("SpiderCarrier needs a CarrySocket assigned!");
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying == false) return;

            Gizmos.color = Color.cyan;
            var origin = transform.position + Vector3.up * 0.2f;
            var dir = _agent != null && _agent.velocity.sqrMagnitude > 0.01f
                ? _agent.velocity.normalized
                : transform.forward;
            Gizmos.DrawLine(origin, origin + dir * _lookAhead);
            Gizmos.DrawWireSphere(origin + dir * _lookAhead, _detectionRadius);
        }
#endif

        void TickPatrol()
        {
            var origin = transform.position + Vector3.up * 0.2f;
            var dir = _agent.velocity.sqrMagnitude > 0.01f ? _agent.velocity.normalized : transform.forward;

            if (Physics.SphereCast(origin, _detectionRadius, dir, out var hit, _lookAhead))
                if (hit.collider.gameObject.GetComponent<Carryable>()) {
                    _currentTarget = hit.collider.gameObject;
                    _patrol.enabled = false;
                    _state = State.Acquire;
                    _originalStopping = _agent.stoppingDistance;
                    _agent.stoppingDistance = _grabDistance;
                    var approach = GetApproachPoint(_currentTarget.transform, _grabDistance);
                    _agent.SetDestination(approach);
                }
        }

        void TickAcquire()
        {
            if (_currentTarget == null) {
                ResumePatrol();
                return;
            }

            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.05f && !_foundObject) {
                _foundObject = true;
                StartCoroutine(DoPickup(_currentTarget));
            }
        }

        Vector3 GetApproachPoint(Transform target, float offset)
        {
            var dir = target.position - transform.position;
            dir.y = 0f;
            dir = dir.normalized;

            var point = target.position - dir * offset;

            NavMesh.SamplePosition(point, out var hit, 1f, NavMesh.AllAreas);
            return hit.position;
        }

        IEnumerator DoPickup(GameObject obj)
        {
            _agent.isStopped = true;
            yield return new WaitForSeconds(_waitBeforePickup);
            if (obj.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            if (obj.TryGetComponent(out Collider col)) col.enabled = false;

            var startPos = obj.transform.position;
            var startRot = obj.transform.rotation;
            var endPos = _carrySocket.position;
            var endRot = _carrySocket.rotation;

            float t = 0f;
            while (t < 1f) {
                t += Time.deltaTime / _moveTime;
                float yArc = _heightCurve.Evaluate(t) * 0.15f;
                obj.transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * yArc;
                obj.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
                yield return null;
            }

            obj.transform.SetParent(_carrySocket, true);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            _agent.isStopped = false;
            _currentTarget = null;
            _state = State.Carrying;
            ResumePatrol();
        }

        void ResumePatrol()
        {
            _patrol.enabled = true;
            _agent.stoppingDistance = _originalStopping;
            _patrol.RepathToCurrentTarget();
            _state = State.Patrol;
        }

        enum State { Patrol, Acquire, Carrying }
    }
}