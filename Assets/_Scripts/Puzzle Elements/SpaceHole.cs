using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Applies a pull force to Rigidbodies within a defined hemispherical area.
    /// The force strength is modulated by distance using an AnimationCurve.
    /// </summary>
    public class SpaceHole : MonoBehaviour
    {
        [SerializeField] float _pullRadius = 20f;
        [SerializeField] float _pullForce = 20f;
        [SerializeField] LayerMask _affectedLayers = -1;
        [SerializeField] AnimationCurve _forceCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

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
                if (rb && rb.gameObject != gameObject && Vector3.Dot(
                        (hitColliders[i].transform.position - transform.position).normalized,
                        transform.TransformDirection(Vector3.forward.normalized)
                    ) > 0) {
                    var pullVector = transform.position - rb.position;
                    pullVector.Normalize();

                    float forceMultiplier = 1.0f;
                    if (_forceCurve != null && _forceCurve.keys.Length > 0) {
                        forceMultiplier = _forceCurve.Evaluate(Mathf.Clamp01(pullVector.magnitude / _pullRadius));
                    }

                    rb.AddForce(
                        pullVector * (_pullForce * forceMultiplier),
                        ForceMode.Acceleration
                    );
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _pullRadius);
            Gizmos.DrawLine(transform.position, transform.TransformDirection(Vector3.forward));
        }
    }
}