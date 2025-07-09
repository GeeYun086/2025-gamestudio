using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Applies a pull force to Rigidbodies within a defined hemispherical area.
    /// The force pulls objects towards the closest point of the GO.
    /// </summary>
    [RequireComponent(typeof(MeshCollider))]
    public class SpaceHole : MonoBehaviour
    {
        [SerializeField] float _pullRadius = 20f;
        [SerializeField] float _pullForce = 50f;
        [SerializeField] LayerMask _affectedLayers = -1;
        [SerializeField] AnimationCurve _forceCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

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
                float forceMultiplier = _forceCurve.Evaluate(1f - Mathf.Clamp01(pullVector.magnitude / _pullRadius));
                rb.AddForce(
                    pullVector.normalized * (_pullForce * forceMultiplier),
                    ForceMode.Acceleration
                );
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _pullRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * _pullRadius);
        }
    }
}