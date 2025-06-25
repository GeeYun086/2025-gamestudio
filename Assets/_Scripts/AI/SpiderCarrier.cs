using System.Collections;
using GravityGame.Puzzle_Elements;
using UnityEngine;
using UnityEngine.Serialization;

namespace GravityGame.AI
{
    [RequireComponent(typeof(SurfaceWaypointPatrol))]
    public class SpiderCarrier : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] float _detectForward = 1.2f;
        [SerializeField] float _detectRadius = 0.4f;
        [SerializeField] LayerMask _carryMask = ~0;
        [Header("Pick-up distance")]
        [SerializeField] float _stopDist   = 0.4f;
        [Header("Animation")]
        [SerializeField] float _waitBefore = 0.3f;
        [SerializeField] float _moveTime   = 0.25f;
        [SerializeField] AnimationCurve   _height = AnimationCurve.EaseInOut(0,0,1,1);
        
        [SerializeField] Transform _carrySocket;

        SurfaceWaypointPatrol _mover;
        GameObject _target;
        enum State { Free, Approach, Carry } State _state = State.Free;

        void Awake()  => _mover = GetComponent<SurfaceWaypointPatrol>();

        void Update()
        {
            switch (_state)
            {
                case State.Free:
                    var origin = transform.position + Vector3.up * 0.2f;
                    if (Physics.SphereCast(origin, _detectRadius, transform.forward, out var hit, _detectForward) &&
                        hit.collider.gameObject.GetComponent<Carryable>())
                    {
                        Debug.Log(hit.collider.gameObject.name);
                        _target = hit.collider.gameObject;
                        _mover.SetPaused(true);
                        _state = State.Approach;
                    }
                    break;

                case State.Approach:
                    if (Vector3.Distance(transform.position,
                                         _target.transform.position) <= _stopDist)
                        StartCoroutine(DoPickup(_target));
                    break;
            }
        }

        IEnumerator DoPickup(GameObject obj)
        {
            _state = State.Carry;

            yield return new WaitForSeconds(_waitBefore);
            
            Vector3 startP = obj.transform.position;
            Quaternion startR = obj.transform.rotation;
            Vector3 endP = _carrySocket.position;
            Quaternion endR = _carrySocket.rotation;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / _moveTime;
                float h = _height.Evaluate(t) * 0.15f;
                obj.transform.position =
                    Vector3.Lerp(startP, endP, t) + Vector3.up * h;
                obj.transform.rotation =
                    Quaternion.Slerp(startR, endR, t);
                yield return null;
            }

            var joint = obj.AddComponent<FixedJoint>();
            joint.connectedBody = _carrySocket.GetComponent<Rigidbody>();
            joint.enableCollision = false;
            obj.transform.SetPositionAndRotation(endP, endR);

            _mover.SetPaused(false);
            _state = State.Free;
        }
        

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;

            // front-sphere centre (half-way along the cast)
            Vector3 cen = transform.position + transform.forward * _detectForward * 0.5f;
            // draw cylinder-ish guide: two spheres + lines
            Gizmos.DrawWireSphere(cen + transform.forward * _detectForward * 0.5f, _detectRadius);
            Gizmos.DrawWireSphere(cen - transform.forward * _detectForward * 0.5f, _detectRadius);
            Gizmos.DrawLine(cen + transform.up * _detectRadius,
                cen + transform.up * _detectRadius + transform.forward * _detectForward);
            Gizmos.DrawLine(cen - transform.up * _detectRadius,
                cen - transform.up * _detectRadius + transform.forward * _detectForward);
        }
#endif

    }
}
