using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace GravityGame
{
    /// <summary>
    /// Moves a NavMeshAgent back and forth through a list of waypoints,
    /// reversing direction when it reaches either end
    /// </summary>
    public class PingPongNavMeshPatrol : MonoBehaviour
    {
        public enum PatrolMode { PingPong, Loop }

        [Header("Path definition")]
        [Tooltip("Waypoint parent")]
        public Transform waypointsRoot;

        [Header("Patrol behaviour")]
        public PatrolMode patrolMode = PatrolMode.PingPong;
        public float waitAtEnds = 0.5f;
        public float waitAtPoints = 0f;

        NavMeshAgent agent;
        Transform[] pts;
        int index = 0;
        int dir = 1; // +1 forward, –1 backward
        bool waiting;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (waypointsRoot == null) {
                Debug.LogError($"{name}: Patrol script needs a waypoints root!");
                enabled = false;
                return;
            }

            pts = new Transform[waypointsRoot.childCount];
            for (int i = 0; i < pts.Length; i++) pts[i] = waypointsRoot.GetChild(i);
        }

        void Start() => GoTo(index);

        void Update()
        {
            if (waiting || pts.Length == 0) return;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                StartCoroutine(NextTarget());
        }

        IEnumerator NextTarget()
        {
            waiting = true;

            if (waitAtPoints > 0) yield return new WaitForSeconds(waitAtPoints);

            switch (patrolMode) {
                case PatrolMode.PingPong:
                    index += dir;
                    if (index >= pts.Length) {
                        dir = -1;
                        index = pts.Length - 2;
                    } else if (index < 0) {
                        dir = 1;
                        index = 1;
                    }
                    if ((index == 0 || index == pts.Length - 1) && waitAtEnds > 0)
                        yield return new WaitForSeconds(waitAtEnds);
                    break;

                case PatrolMode.Loop:
                    index = (index + 1) % pts.Length;
                    if (index == 0 && waitAtEnds > 0)
                        yield return new WaitForSeconds(waitAtEnds);
                    break;
            }

            GoTo(index);
            waiting = false;
        }

        void GoTo(int i)
        {
            if (!NavMesh.SamplePosition(pts[i].position, out var hit, 1f, NavMesh.AllAreas))
                Debug.LogWarning($"Waypoint {pts[i].name} is off the NavMesh!");
            else
                agent.SetDestination(hit.position);
        }
    }
}