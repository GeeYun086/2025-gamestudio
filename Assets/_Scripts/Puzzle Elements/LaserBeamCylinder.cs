using UnityEngine;
using GravityGame.Player;  // Adjust this namespace if your PlayerHealth is elsewhere

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LaserBeamCylinder : MonoBehaviour
{
    [Header("Beam Settings")]
    [Tooltip("Maximum length of the laser if nothing blocks it.")]
    public float maxDistance = 20f;

    [Tooltip("Which layers block the beam (Default, Player, and Cube).")]
    public LayerMask obstacleMask;

    [Header("Visual Settings")]
    [Tooltip("Radius of the beam cylinder (half of its diameter).")]
    public float beamRadius = 0.05f;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        // 1) Ensure a Cylinder mesh is assigned on this MeshFilter
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshFilter.sharedMesh == null)
        {
            // Create a temporary Cylinder primitive to extract its mesh
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Mesh cylMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(temp);
            meshFilter.sharedMesh = cylMesh;
        }

        // 2) Ensure there is a simple red, unlit material on this MeshRenderer
        if (meshRenderer.sharedMaterial == null)
        {
            Material redMat = new Material(Shader.Find("Sprites/Default"));
            redMat.color = Color.red;
            meshRenderer.sharedMaterial = redMat;
        }
    }

    void Update()
    {
        // 3) Cast a ray forward from the emitter’s origin (its parent),
        //    offset slightly so the ray doesn't start inside a blocking collider.
        Transform emitterOrigin = transform.parent;
        Vector3 rayStart = emitterOrigin.position + emitterOrigin.forward * 0.01f;
        Ray ray = new Ray(rayStart, emitterOrigin.forward);

        RaycastHit hitInfo;
        float beamLength = maxDistance;

        // 4) Raycast against Default, Player, and Cube layers
        if (Physics.Raycast(ray, out hitInfo, maxDistance, obstacleMask))
        {
            beamLength = hitInfo.distance;

            // 5a) If the hit collider is tagged "Player", kill them:
            if (hitInfo.collider.CompareTag("Player"))
            {
                PlayerHealth ph = hitInfo.collider.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(ph.CurrentHealth);
                }
            }
            // 5b) If the hit collider is on the Cube or Default layer,
            //      we simply shorten the beam to 'hitInfo.distance'.
        }

        // 6) Position & scale this child (BeamVisual) so it spans exactly beamLength
        //    Unity’s Cylinder is 2 units tall along its local Y axis. We want its length along Z,
        //    so we rotate it by 90° around X (pitch).
        Quaternion cylinderRotation = Quaternion.Euler(90f, 0f, 0f);
        Vector3 newScale = new Vector3(beamRadius, beamLength * 0.5f, beamRadius);
        Vector3 localMidpoint = new Vector3(0f, 0f, beamLength * 0.5f);

        transform.localRotation = cylinderRotation;
        transform.localScale    = newScale;
        transform.localPosition = localMidpoint;
    }

    // Optional: show the full maxDistance in the Scene view when this object is selected
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
