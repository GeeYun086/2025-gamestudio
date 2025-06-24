using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Applies a pull force to Rigidbodies towards the surface of this objects collider.
    /// The force strength is modulated by distance using an AnimationCurve.
    /// </summary>
    [RequireComponent(typeof(MeshCollider))]
    public class SpaceHole : MonoBehaviour
    {
        [SerializeField] float _pullRadius = 20f;
        [SerializeField] float _pullForce = 50f;
        [SerializeField] bool _requireLineOfSight = true;
        [SerializeField] LayerMask _affectedLayers = -1;
        [SerializeField] AnimationCurve _forceCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        MeshCollider _meshCollider;

        void Awake()
        {
            _meshCollider = GetComponent<MeshCollider>();
            _meshCollider.convex = true;
            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer) meshRenderer.enabled = false;
        }

        void FixedUpdate()
        {
            var hitColliders = new Collider[100];
            int colliders = Physics.OverlapSphereNonAlloc(
                transform.position,
                _pullRadius,
                hitColliders,
                _affectedLayers
            );

            for (int i = 0; i < colliders; i++) {
                var rb = hitColliders[i].GetComponent<Rigidbody>();
                if (!rb || rb.gameObject == gameObject) continue;
                if (Vector3.Dot(rb.position - transform.position, transform.forward) <= 0) continue;

                var pullVector = _meshCollider.ClosestPoint(rb.position) - rb.position;
                float distance = pullVector.magnitude;
                if (distance > _pullRadius) continue;

                if (_requireLineOfSight) {
                    if (Physics.Raycast(rb.position, pullVector.normalized, out var hit, distance, ~(1 << rb.gameObject.layer)))
                        if (hit.collider != _meshCollider) continue;
                }

                rb.AddForce(
                    pullVector.normalized * (_pullForce * _forceCurve.Evaluate(distance / _pullRadius)),
                    ForceMode.Acceleration
                );
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _pullRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * 3);
        }
    }
}