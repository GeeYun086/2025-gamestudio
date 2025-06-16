using UnityEngine;

namespace GravityGame
{
    [RequireComponent(typeof(Rigidbody))]
    public class ImpactEffect : MonoBehaviour
    {
        Rigidbody _rigidbody;
        public float ImpactVelocity = 1;
        public GameObject ImpactParticle;
        ImpactPool _pool;

        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
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