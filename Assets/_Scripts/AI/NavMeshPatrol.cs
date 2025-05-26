using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace GravityGame
{
    /// <summary>
    ///     Moves a NavMeshAgent back and forth through a list of waypoints,
    ///     reversing direction when it reaches either end
    /// </summary>
    public class NavMeshPatrol : MonoBehaviour
    {
        public enum PatrolMode { PingPong, Loop }
        
        [Header("Path definition")]
        [Tooltip("Waypoint parent")]
        public Transform WaypointsRoot;

        [Header("Patrol behaviour")]
        public PatrolMode SelectedMode = PatrolMode.PingPong;
        public float WaitAtEnds = 0.5f;
        public float WaitAtPoints;

        NavMeshAgent _agent;
        int _dir = 1; // +1 forward, –1 backward
        int _index;
        Transform[] _pts;
        bool _waiting;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (WaypointsRoot == null) {
                Debug.LogError($"{name}: Patrol script needs a waypoints root!");
                enabled = false;
                return;
            }

            _pts = new Transform[WaypointsRoot.childCount];
            for (int i = 0; i < _pts.Length; i++) _pts[i] = WaypointsRoot.GetChild(i);
        }

        void Start() => GoTo(_index);

        void Update()
        {
            if (_waiting || _pts.Length == 0) return;

            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                StartCoroutine(NextTarget());
        }

        IEnumerator NextTarget()
        {
            _waiting = true;

            if (WaitAtPoints > 0) yield return new WaitForSeconds(WaitAtPoints);

            switch (SelectedMode) {
                case PatrolMode.PingPong:
                    _index += _dir;
                    if (_index >= _pts.Length) {
                        _dir = -1;
                        _index = _pts.Length - 2;
                    } else if (_index < 0) {
                        _dir = 1;
                        _index = 1;
                    }
                    if ((_index == 0 || _index == _pts.Length - 1) && WaitAtEnds > 0)
                        yield return new WaitForSeconds(WaitAtEnds);
                    break;

                case PatrolMode.Loop:
                    _index = (_index + 1) % _pts.Length;
                    if (_index == 0 && WaitAtEnds > 0)
                        yield return new WaitForSeconds(WaitAtEnds);
                    break;
            }

            GoTo(_index);
            _waiting = false;
        }

        void GoTo(int i)
        {
            if (!NavMesh.SamplePosition(_pts[i].position, out var hit, 1f, NavMesh.AllAreas))
                Debug.LogWarning($"Waypoint {_pts[i].name} is off the NavMesh!");
            else
                _agent.SetDestination(hit.position);
        }
    }
}