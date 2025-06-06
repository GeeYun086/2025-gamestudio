// LaserBeamCylinder.cs
// Handles a laser beam that damages the player for 80% health on contact
// and is physically blocked by Default‐layer geometry and cubes.

using UnityEngine;
using GravityGame.Player;  // Adjust if PlayerHealth is in a different namespace

namespace GravityGame.PuzzleElements
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class LaserBeamCylinder : MonoBehaviour
    {
        [Header("Beam Settings")]
        [Tooltip("Maximum length of the laser if nothing blocks it.")]
        public float maxDistance = 20f;

        [Tooltip("Which layers block the beam (Default and Cube).")]
        public LayerMask obstacleMask;

        [Header("Damage Settings")]
        [Tooltip("Percentage of current health to deduct when the player is hit.")]
        [Range(0f, 1f)]
        public float damageFraction = 0.8f;

        [Header("Visual Settings")]
        [Tooltip("Radius of the beam cylinder (half its diameter).")]
        public float beamRadius = 0.05f;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private CapsuleCollider beamCollider;

        private int playerLayerMask;

        void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();

            // Assign Cylinder mesh if none
            if (meshFilter.sharedMesh == null)
            {
                GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Mesh cylMesh = temp.GetComponent<MeshFilter>().sharedMesh;
                DestroyImmediate(temp);
                meshFilter.sharedMesh = cylMesh;
            }

            // Assign red, unlit material if none
            if (meshRenderer.sharedMaterial == null)
            {
                Material redMat = new Material(Shader.Find("Sprites/Default"));
                redMat.color = Color.red;
                meshRenderer.sharedMaterial = redMat;
            }

            // Configure CapsuleCollider for physical blocking
            beamCollider = GetComponent<CapsuleCollider>();
            beamCollider.isTrigger = false;
            beamCollider.direction = 2; // Z axis

            // Cache Player layer mask
            playerLayerMask = 1 << LayerMask.NameToLayer("Player");
        }

        void Update()
        {
            Transform emitterOrigin = transform.parent;
            Vector3 rayStart = emitterOrigin.position + emitterOrigin.forward * 0.01f;
            Ray ray = new Ray(rayStart, emitterOrigin.forward);

            float beamLength = maxDistance;

            int combinedMask = obstacleMask | playerLayerMask;
            if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, combinedMask))
            {
                beamLength = hitInfo.distance;

                if (hitInfo.collider.CompareTag("Player"))
                {
                    PlayerHealth ph = hitInfo.collider.GetComponent<PlayerHealth>();
                    if (ph != null)
                    {
                        float damageAmount = ph.CurrentHealth * damageFraction;
                        ph.TakeDamage(damageAmount);
                    }
                }
            }

            // Update visual cylinder
            Quaternion cylRotation = Quaternion.Euler(90f, 0f, 0f);
            Vector3 newScale = new Vector3(beamRadius, beamLength * 0.5f, beamRadius);
            Vector3 localMidpoint = new Vector3(0f, 0f, beamLength * 0.5f);

            transform.localRotation = cylRotation;
            transform.localScale = newScale;
            transform.localPosition = localMidpoint;

            // Update collider to match visual
            beamCollider.radius = beamRadius;
            beamCollider.height = beamLength;
            beamCollider.center = new Vector3(0f, 0f, beamLength * 0.5f);
        }

        void OnDrawGizmosSelected()
        {
            if (transform.parent != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(
                    transform.parent.position,
                    transform.parent.position + transform.parent.forward * maxDistance
                );
            }
        }
    }
}
