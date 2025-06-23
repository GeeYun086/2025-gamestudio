using UnityEngine;

namespace GravityGame
{
    /// <summary>
    ///     Implements Impact effect:
    ///     spawns particle effects system at point of impact, taking object from object pool to improve performance (5 systems per effect system)
    ///     Impact Particle is set by prefab particle system
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ImpactEffect : MonoBehaviour
    {
        public float ImpactVelocity = 1;
        public GameObject ImpactParticle;
        ImpactPool _pool;

        void Start()
        {
            _pool = GetComponent<ImpactPool>();
            _pool.Prefab = ImpactParticle;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude >= ImpactVelocity) {
                var contact = collision.GetContact(0);
                var impact = _pool.GetObject();
                if (impact != null) {
                    impact.GetComponent<PoolReset>().ImpactPool = _pool;
                    impact.transform.position = contact.point;
                    impact.transform.forward = contact.normal;
                }
            }
        }
    }
}