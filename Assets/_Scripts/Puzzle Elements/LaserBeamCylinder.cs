using UnityEngine;
using GravityGame.Player;
using UnityEngine.Serialization; // Adjust this namespace if your PlayerHealth is in a different namespace

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LaserBeamCylinder : MonoBehaviour
{
    [FormerlySerializedAs("maxDistance")]
    [Header("Beam Settings")]
    [Tooltip("Maximum length of the laser if nothing blocks it.")]
    public float MaxDistance = 20f;

    [FormerlySerializedAs("obstacleMask")] [Tooltip("Layers that block the beam (must include Player and Cube).")]
    public LayerMask ObstacleMask;

    [FormerlySerializedAs("beamRadius")]
    [Header("Visual Settings")]
    [Tooltip("Radius of the beam cylinder (half of the diameter).")]
    public float BeamRadius = 0.05f;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    void Awake()
    {
        // 1) Ensure the cylinder mesh is assigned
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

        if (_meshFilter.sharedMesh == null)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Mesh cylMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(temp);
            _meshFilter.sharedMesh = cylMesh;
        }

        // 2) Ensure there is a red unlit material
        if (_meshRenderer.sharedMaterial == null)
        {
            Material redMat = new Material(Shader.Find("Sprites/Default"));
            redMat.color = Color.red;
            _meshRenderer.sharedMaterial = redMat;
        }
    }

    void Update()
    {
        // Cast a ray from the emitter origin (parent) forward
        Transform emitterOrigin = transform.parent;
        Ray ray = new Ray(emitterOrigin.position, emitterOrigin.forward);
        RaycastHit hitInfo;
        float beamLength = MaxDistance;

        if (Physics.Raycast(ray, out hitInfo, MaxDistance, ObstacleMask))
        {
            beamLength = hitInfo.distance;

            // If it’s the player, kill them
            if (hitInfo.collider.CompareTag("Player"))
            {
                PlayerHealth ph = hitInfo.collider.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(ph.CurrentHealth);
                }
            }
            // If it’s a cube (layer “Cube”), we just shorten the beam
        }

        // Position & scale this GameObject (BeamVisual child) so it stretches from origin → beamLength
        Quaternion cylinderRotation = Quaternion.Euler(90f, 0f, 0f);
        Vector3 newScale = new Vector3(BeamRadius, beamLength * 0.5f, BeamRadius);
        Vector3 localMidpoint = new Vector3(0f, 0f, beamLength * 0.5f);

        transform.localRotation = cylinderRotation;
        transform.localScale    = newScale;
        transform.localPosition = localMidpoint;
    }

    // Optional: show maxDistance line in Scene View when selected
    void OnDrawGizmosSelected()
    {
        if (transform.parent != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.parent.position, transform.parent.position + transform.parent.forward * MaxDistance);
        }
    }
}
