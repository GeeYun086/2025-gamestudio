using System.Collections;
using System.Linq;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    // This component makes a GameObject "breakable" upon sufficient collision force.
    // When it breaks, it replaces itself with a shattered version and assigns proper settings to debris.

    [RequireComponent(typeof(Collider))]
    public class Breakable : MonoBehaviour
    {
        [Header("Destruction Settings")]
        public float BreakForceThreshold = 9f; // Minimum velocity required to break the object.
        public GameObject IntactVersion;
        public GameObject BrokenVersionPrefab;

        [Header("Debris Settings")]
        public float DebrisLifetime = 2.5f;
        public float BreakOffThreshold = 2.5f;
        public string DebrisLayer = "GlassDebris"; // Layer assigned to the broken debris for effects/collision filtering.
        public bool PreventDestructionByPlayerCollision = true;

        void OnCollisionEnter(Collision collision)
        {
            if (PreventDestructionByPlayerCollision && collision.gameObject.layer == LayerMask.NameToLayer("Player")) return;
            if (collision.impulse.magnitude < BreakForceThreshold) return;
            Break(collision.GetContact(0).point, collision.relativeVelocity);
        }

        public void Break(Vector3 contact, Vector3 velocity)
        {
            // Break
            GetComponent<Collider>().enabled = false;
            IntactVersion.SetActive(false);
                
            var brokenInstance = Instantiate(
                BrokenVersionPrefab,
                transform
            );

            // Set all rigidbodies in the shattered version to the correct debris layer,
            // and schedule them to be destroyed after a set lifetime
            var brokenRbs = brokenInstance.GetComponentsInChildren<Rigidbody>().ToList();
            int index = 0;
            foreach (var rb in brokenRbs.OrderBy(rb => Vector3.Distance(rb.position, contact))) {
                rb.gameObject.layer = LayerMask.NameToLayer(DebrisLayer);
                
                
                float minLifetime = DebrisLifetime * 0.25f;
                float lifetime = (float)index / brokenRbs.Count;
                lifetime *= (DebrisLifetime - minLifetime) + minLifetime;                
                lifetime *= Random.Range(0.8f, 1.2f);
                
                // rb.AddExplosionForce(magnitude, contact, magnitude);
                var distance = (rb.position - contact);
                var v = velocity / distance.magnitude;
                rb.AddForceAtPosition(v, contact, ForceMode.VelocityChange);
                if (v.magnitude > BreakOffThreshold) {
                    rb.useGravity = true;
                    // rb.AddForceAtPosition(magnitude * (contact - rb.position), contact, ForceMode.VelocityChange);
                } else {
                    rb.isKinematic = true;
                    StartCoroutine(Fall());
                    rb.useGravity = false;
                }
                
                
                Destroy(rb.gameObject, lifetime);
                index++;

                IEnumerator Fall()
                {
                    yield return new WaitForSeconds(lifetime*0.5f);
                    if (rb) {
                        rb.isKinematic = false;
                        rb.useGravity = true;
                    }
                }
            }
        }
    }
}