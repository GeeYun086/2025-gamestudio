using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Applies a pull force to Rigidbodies within a defined hemispherical area.
    /// The force strength is modulated by distance using an AnimationCurve.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SpaceHole : MonoBehaviour
    {
        [SerializeField] float _pullRadius = 20f;
        [SerializeField] float _pullForce = 50f;
        [SerializeField] LayerMask _affectedLayers = -1;
        [SerializeField] AnimationCurve _forceCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        float _calculatedHoleRadius;

        void Awake()
        {
            var scale = transform.lossyScale;
            if (TryGetComponent<SphereCollider>(out var sphere)) {
                _calculatedHoleRadius = sphere.radius * Mathf.Max(scale.x, scale.y, scale.z);
            } else if (TryGetComponent<BoxCollider>(out var box)) {
                _calculatedHoleRadius = Mathf.Max(box.size.x * scale.x, box.size.y * scale.y) / 2f;
            }
        }

        void FixedUpdate()
        {
            var pullPlane = new Plane(transform.forward, transform.position);

            var hitColliders = new Collider[100];
            int collidersFound = Physics.OverlapSphereNonAlloc(
                transform.position,
                _pullRadius,
                hitColliders,
                _affectedLayers
            );

            for (int i = 0; i < collidersFound; i++) {
                var rb = hitColliders[i].GetComponent<Rigidbody>();
                if (!rb || rb.gameObject == gameObject) continue;
                if (Vector3.Dot(rb.position - transform.position, transform.forward) <= 0) continue;

                var vectorFromCenter = pullPlane.ClosestPointOnPlane(rb.position) - transform.position;
                var target = vectorFromCenter.sqrMagnitude > _calculatedHoleRadius * _calculatedHoleRadius
                    ? transform.position + vectorFromCenter.normalized * _calculatedHoleRadius
                    : pullPlane.ClosestPointOnPlane(rb.position);

                var pullVector = target - rb.position;
                rb.AddForce(
                    pullVector.normalized * (_pullForce * _forceCurve.Evaluate(1f - Mathf.Clamp01(pullVector.magnitude / _pullRadius))),
                    ForceMode.Acceleration
                );
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _pullRadius);
        }
    }
}