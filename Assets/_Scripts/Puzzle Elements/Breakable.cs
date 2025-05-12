using UnityEngine;

namespace GravityGame
{
    public class Breakable : MonoBehaviour
    {
        public float BreakForceThreshold = 10f;
        public GameObject BrokenVersionPrefab;

        void OnCollisionEnter(Collision collision)
        {
            float impactForce = collision.relativeVelocity.magnitude * collision.rigidbody.mass;

            if (impactForce > BreakForceThreshold)
            {
                Break();
            }

            if (collision.relativeVelocity.y > BreakForceThreshold)
            {
                Break();
            }
        }

        void Break()
        {
            if (BrokenVersionPrefab != null)
            {
                Instantiate(BrokenVersionPrefab, transform.position, transform.rotation);
            }

            Destroy(gameObject);
        }
    }
}
