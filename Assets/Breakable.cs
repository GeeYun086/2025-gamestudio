using System;
using Unity.VisualScripting;
using UnityEngine;

namespace GravityGame
{
    public class Breakable : MonoBehaviour
    {
        public float breakForceThreshold = 10f;
        public GameObject brokenVersionPrefab;

        void OnCollisionEnter(Collision collision)
        {
            float impactForce = collision.relativeVelocity.magnitude * collision.rigidbody.mass;

            if (impactForce > breakForceThreshold)
            {
                Break();
            }

            if (collision.relativeVelocity.y > breakForceThreshold)
            {
                Break();
            }
        }

        void Break()
        {
            if (brokenVersionPrefab != null)
            {
                Instantiate(brokenVersionPrefab, transform.position, transform.rotation);
            }

            Destroy(gameObject);
        }
    }
}
