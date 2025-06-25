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
        public GameObject BrokenVersionPrefab; 

        [Header("Debris Settings")]
        public float DebrisLifetime = 2.5f;
        public string DebrisLayer = "GlassDebris"; // Layer assigned to the broken debris for effects/collision filtering.

        bool _alreadyBroken;// Prevents multiple breaks from happening.

        
        void OnCollisionEnter(Collision collision)
        {
           
            if (_alreadyBroken || collision.rigidbody == null) return;

            // Extract physical data from the collision
           
            float mass = collision.rigidbody.mass;
            float velocity = collision.relativeVelocity.magnitude;

           
            if (mass < 1f && velocity < 5f)
                return;

           

            
            if (velocity >= BreakForceThreshold)
            {
                Break();
            }
        }

        // Ensures the instantiated object's world scale matches the original object’s world scale.
        private void MatchWorldScale(Transform target, Vector3 desiredWorldScale)
        {
            Vector3 parentScale = target.parent != null ? target.parent.lossyScale : Vector3.one;

            // Adjust the localScale based on parent scale to match the desired world scale
            target.localScale = new Vector3(
                desiredWorldScale.x / parentScale.x,
                desiredWorldScale.y / parentScale.y,
                desiredWorldScale.z / parentScale.z
            );
        }

        // Handles breaking the object and spawning its shattered version
        public void Break()
        {
            _alreadyBroken = true; 

            // Attempt to find the original glass child, usually the visible part
            Transform glassChild = transform.Find("glass_base");

            if (BrokenVersionPrefab != null)
            {
                // Use the child glass's position, rotation, and world scale if found,
                // otherwise fall back to this object's transform
                Vector3 spawnPosition = glassChild != null ? glassChild.position : transform.position;
                Quaternion spawnRotation = glassChild != null ? glassChild.rotation : transform.rotation;
                Vector3 desiredScale = glassChild != null ? glassChild.lossyScale : transform.lossyScale;

                // Instantiate the shattered version at the right position and rotation
                GameObject brokenInstance = Instantiate(
                    BrokenVersionPrefab,
                    spawnPosition,
                    spawnRotation
                );

               
                brokenInstance.transform.localScale = glassChild.localScale;

                // Adjust the scale of the shattered prefab so it matches the original glass visually
                MatchWorldScale(brokenInstance.transform, desiredScale);

                // Set all rigidbodies in the shattered version to the correct debris layer,
                // and schedule them to be destroyed after a set lifetime
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
