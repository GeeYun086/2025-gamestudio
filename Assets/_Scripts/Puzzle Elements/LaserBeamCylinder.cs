using UnityEngine;
using GravityGame.Player;
using UnityEngine.Serialization; // Adjust if your namespace is different

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LaserBeamCylinder : MonoBehaviour
{
    [FormerlySerializedAs("maxDistance")] [Header("Beam Settings")]
    public float MaxDistance = 20f;
    public LayerMask ObstacleMask;

    [FormerlySerializedAs("beamRadius")] [Header("Cylinder Visual")]
    public float BeamRadius = 0.05f;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    void Awake()
    {
        // 1) Ensure we have a Cylinder mesh
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

        if (_meshFilter.sharedMesh == null)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Mesh cylMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(temp);
            _meshFilter.sharedMesh = cylMesh;
        }

        if (_meshRenderer.sharedMaterial == null)
        {
            Material redMat = new Material(Shader.Find("Sprites/Default"));
            redMat.color = Color.red;
            _meshRenderer.sharedMaterial = redMat;
        }

        // No need for a collider, so remove any calls to one
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo;
        float beamLength = MaxDistance;

        if (Physics.Raycast(ray, out hitInfo, MaxDistance, ObstacleMask))
        {
            beamLength = hitInfo.distance;

            if (hitInfo.collider.CompareTag("Player"))
            {
                PlayerHealth ph = hitInfo.collider.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(ph.CurrentHealth);
                }
            }
        }

        // Position & scale the cylinder
        Vector3 localMidpoint = Vector3.forward * (beamLength / 2f);
        Vector3 newScale = new Vector3(BeamRadius, beamLength / 2f, BeamRadius);
        Quaternion cylinderRotation = Quaternion.Euler(90f, 0f, 0f);

        // Apply to the same GameObject
        transform.localScale = newScale;
        transform.localRotation = cylinderRotation;
        transform.localPosition = localMidpoint;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * MaxDistance);
    }
}
