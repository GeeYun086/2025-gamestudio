using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    [RequireComponent(typeof(Collider))]
    public class Breakable : MonoBehaviour
    {
        [Header("Destruction Settings")]
        public float BreakForceThreshold = 9f;
        public GameObject BrokenVersionPrefab;

        [Header("Debris Settings")]
        public float DebrisLifetime = 2.5f;
        public string DebrisLayer = "GlassDebris";
        


        private bool alreadyBroken = false;

        void OnCollisionEnter(Collision collision)
        {
            if (alreadyBroken || collision.rigidbody == null) return;

            
            float impactForce = collision.impulse.magnitude;
            float mass = collision.rigidbody.mass;
            float velocity = collision.relativeVelocity.magnitude;

            
            if (mass < 1f && velocity < 5f)
                return;

            Debug.Log(velocity);

            if (velocity >= BreakForceThreshold)
            {
                Break();
            }
        }
        
        private void MatchWorldScale(Transform target, Vector3 desiredWorldScale)
        {
            Vector3 parentScale = target.parent != null ? target.parent.lossyScale : Vector3.one;

            // Divide desired world scale by parent scale to get proper local scale
            target.localScale = new Vector3(
                desiredWorldScale.x / parentScale.x,
                desiredWorldScale.y / parentScale.y,
                desiredWorldScale.z / parentScale.z
            );
        }


        public void Break()
        {
            alreadyBroken = true;

            Transform glassChild = transform.Find("glass_base");
            if (BrokenVersionPrefab != null)
            {
                Vector3 spawnPosition = glassChild != null ? glassChild.position : transform.position;
                Quaternion spawnRotation = glassChild != null ? glassChild.rotation : transform.rotation;
                Vector3 desiredScale = glassChild != null ? glassChild.lossyScale : transform.lossyScale;
                
                GameObject brokenInstance = Instantiate(
                    BrokenVersionPrefab,
                    spawnPosition,
                    spawnRotation
                );
                brokenInstance.transform.localScale = glassChild.localScale;
                
                MatchWorldScale(brokenInstance.transform, desiredScale);
                
                foreach (var rb in brokenInstance.GetComponentsInChildren<Rigidbody>())
                {
                    rb.gameObject.layer = LayerMask.NameToLayer(DebrisLayer);
                    Destroy(rb.gameObject, DebrisLifetime); 
                }
            }

            Destroy(gameObject);
        }
    }
}