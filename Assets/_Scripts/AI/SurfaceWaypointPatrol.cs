using UnityEngine;

namespace GravityGame.AI
{
    public class SurfaceWaypointPatrol : MonoBehaviour
    {
        [Header("Path")]
        [Tooltip("Parent that holds all waypoint transforms (in order)")]
        [SerializeField] Transform _waypointsRoot;

        [Tooltip("Stop here before going backwards (Ping-Pong)")]
        [SerializeField] bool _pingPong = true;

        [Header("Locomotion")]
        [SerializeField] float _speed = 2f;
        [SerializeField] float _arriveDist = 0.15f;
        [SerializeField] bool _paused;

        [Header("Surface sticking")]
        [SerializeField] LayerMask _groundMask = ~0;
        [SerializeField] float _rayDist   = 0.6f;
        [SerializeField] float _arcAngle  = 270f;
        [SerializeField] int   _arcRes    = 6;
        [SerializeField] float _rotLerp   = 8f;

        Transform[] _pts;
        int _index;
        int _dir     = 1;           // +1 forward, –1 back

        public void SetPaused(bool v) => _paused = v;
        
        void Awake()
        {
            if (_waypointsRoot == null)
            {
                Debug.LogError("SurfaceWaypointPatrol: need a WaypointsRoot assigned.");
                enabled = false;
                return;
            }

            _pts = new Transform[_waypointsRoot.childCount];
            for (int i = 0; i < _pts.Length; i++) _pts[i] = _waypointsRoot.GetChild(i);
        }

        void Update()
        {
            if (_paused) return;
            if (_pts.Length == 0) return;

            Vector3 toTarget = _pts[_index].position - transform.position;
            if (toTarget.magnitude < _arriveDist)
            {
                if (_pingPong)
                {
                    if (_index == _pts.Length - 1) _dir = -1;
                    else if (_index == 0)         _dir =  1;
                    _index += _dir;
                }
                else
                    _index = (_index + 1) % _pts.Length;

                toTarget = _pts[_index].position - transform.position;
            }
            
            bool wallInFront = Physics.Raycast(transform.position, transform.forward, out var hit, _rayDist, _groundMask);
            if(wallInFront)
                Debug.Log("wall in front? "+ wallInFront);

            /*bool gotSurface =
                Physics.Raycast(transform.position + transform.up * 0.05f,
                    -transform.up,
                    out var hit, _rayDist, _groundMask)
                || ArcCast(transform.position,
                    transform.rotation,
                    _arcAngle,
                    _rayDist,
                    _arcRes,
                    _groundMask,
                    out hit);
            
            if (!gotSurface) return;*/

            Vector3 surfaceNormal = hit.normal;

            /*Quaternion desiredRot =
                Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot,
                _rotLerp * Time.deltaTime);*/

            Vector3 moveDir = Vector3.ProjectOnPlane(toTarget, surfaceNormal).normalized;
            if (moveDir.sqrMagnitude > 0.001f) {
                Quaternion faceDir = Quaternion.LookRotation(moveDir, surfaceNormal);
                //transform.rotation = Quaternion.Slerp(transform.rotation, faceDir, _rotLerp * Time.deltaTime);
            }
            transform.position += moveDir * (_speed * Time.deltaTime);
            
            Vector3 candidate = transform.position + moveDir * (_speed * Time.deltaTime);
            if (Physics.Raycast(candidate + surfaceNormal * 0.05f, -surfaceNormal, out var groundHit, _rayDist, _groundMask)) {
                candidate = groundHit.point + new Vector3(0, 0.65f, 0);
            }
            transform.position = candidate;
        }

        static bool ArcCast(
            Vector3      center,
            Quaternion   rotation,
            float        angleDeg,
            float        radius,
            int          resolution,
            LayerMask    mask,
            out RaycastHit hit)
        {
            rotation *= Quaternion.Euler(-angleDeg * 0.5f, 0, 0);
            for (int i = 0; i < resolution; i++)
            {
                Vector3 a  = center + rotation * Vector3.forward * radius;
                rotation  *= Quaternion.Euler(angleDeg / resolution, 0, 0);
                Vector3 b  = center + rotation * Vector3.forward * radius;
                Vector3 ab = b - a;
                if (Physics.Raycast(a, ab, out hit, ab.magnitude * 1.001f, mask))
                    return true;
            }
            hit = default;
            return false;
        }
        
#if UNITY_EDITOR      // compile in Editor only
        void OnDrawGizmos()
        {
            //if (!Application.isPlaying) return;

            Gizmos.color = Color.red;

            // ── straight-down ray (the first test) ──
            Vector3 downStart = transform.position + transform.up * 0.05f;
            Gizmos.DrawLine(downStart, downStart - transform.up * _rayDist);

            // ── arc fan (second test) ──
            Quaternion rot = transform.rotation * Quaternion.Euler(-_arcAngle * 0.5f, 0, 0);
            for (int i = 0; i < _arcRes; i++)
            {
                Vector3 A = transform.position + rot * Vector3.forward * _rayDist;
                rot *= Quaternion.Euler(_arcAngle / _arcRes, 0, 0);
                Vector3 B = transform.position + rot * Vector3.forward * _rayDist;

                Gizmos.DrawLine(A, B);              // chord
                Gizmos.DrawLine(A, A + (B - A).normalized * 0.05f);   // little direction tick
            }
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * _rayDist);
        }
#endif
        
    }
    
    
}
