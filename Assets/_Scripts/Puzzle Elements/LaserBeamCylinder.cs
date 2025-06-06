using UnityEngine;
using GravityGame.Player;
using UnityEngine.Serialization; // Adjust this if your PlayerHealth lives in a different namespace

[RequireComponent(typeof(Collider))]      // (we need some collider on the emitter, even if unused)
[RequireComponent(typeof(MeshFilter))]    // We’ll add a Cylinder mesh at runtime if needed
[RequireComponent(typeof(MeshRenderer))]
public class LaserBeamCylinder : MonoBehaviour
{
    [FormerlySerializedAs("maxDistance")]
    [Header("Beam Settings")]
    [Tooltip("Maximum length of the laser if nothing blocks it.")]
    public float MaxDistance = 20f;

    [FormerlySerializedAs("obstacleMask")] [Tooltip("Which layers to collide with (must include 'Player' and 'Cube').")]
    public LayerMask ObstacleMask;

    [FormerlySerializedAs("beamRadius")]
    [Header("Cylinder Visual")]
    [Tooltip("Radius of the beam cylinder (half of the cylinder’s width).")]
    public float BeamRadius = 0.05f;

    // Private references
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    void Awake()
    {
        // 1) Ensure we have a Cylinder mesh
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

        // If no mesh is assigned, create a built-in Cylinder
        if (_meshFilter.sharedMesh == null)
        {
            // Unity’s built-in Cylinder primitive has a mesh that we can copy
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Mesh cylMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(temp);
            _meshFilter.sharedMesh = cylMesh;
        }

        // 2) Make sure the object is using a simple unlit red material
        if (_meshRenderer.sharedMaterial == null)
        {
            // Use Sprites/Default so it's an unlit, pure color
            Material redMat = new Material(Shader.Find("Sprites/Default"));
            redMat.color = Color.red;
            _meshRenderer.sharedMaterial = redMat;
        }

        // 3) Disable the collider on this emitter—only the raycast matters, not physical collisions
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void Update()
    {
        // Cast a ray forward from this transform
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo;
        float beamLength = MaxDistance;

        // If the ray hits something on the obstacleMask within maxDistance:
        if (Physics.Raycast(ray, out hitInfo, MaxDistance, ObstacleMask))
        {
            beamLength = hitInfo.distance;

            // If it’s a Player, kill them
            if (hitInfo.collider.CompareTag("Player"))
            {
                PlayerHealth ph = hitInfo.collider.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(ph.CurrentHealth);
                }
            }
            // If it’s a Cube (layer “Cube”), we just shorten the beam here
        }

        // Now position & scale our cylinder so that it stretches from origin → forward * beamLength
        // Unity’s Cylinder primitive by default is 2 units tall, centered at Y=0 (so it extends from -1 to +1 on its local Y axis).
        // We want it to extend from 0 to beamLength along our local Z axis, with a given radius.

        // 1) Compute the midpoint between start (0) and end (beamLength) on the local forward axis
        Vector3 localMidpoint = Vector3.forward * (beamLength / 2f);

        // 2) Set localScale:
        //    - X and Z scale control the cylinder’s radius (because Unity’s Cylinder primitive’s cross‐section is in the XZ plane)
        //    - Y scale controls half the height (2 units in editor = height 2). If we want an actual length of L along local Z, we need to rotate the cylinder so
        //      its length axis is Z (by default it’s Y). We’ll handle rotation next.
        //    - Instead, it’s easier to rotate the cylinder so its length goes along Z, then use Y scale = beamLength/2
        Vector3 newScale = new Vector3(BeamRadius, beamLength / 2f, BeamRadius);

        // 3) Position and rotate:
        //    - Position: worldPos = transform.position + transform.rotation * localMidpoint
        //    - Because the default Cylinder’s “up axis” is Y, we must rotate the cylinder so its “up” becomes our “forward.”
        //      I.e., we need a 90° rotation around the X axis (pitch) so that local Y → local Z.
        Quaternion cylinderRotation = Quaternion.Euler(90f, 0f, 0f);

        // 4) Apply them:
        //    - worldPosition = transform.TransformPoint(localMidpoint)
        transform.rotation = transform.rotation; // keep emitter’s rotation; we'll rotate the mesh below
        transform.position = transform.position; // emitter stays put; we’ll move the mesh in child below

        // Since we want the mesh to be a child of the emitter GameObject for simpler math, let’s ensure that:
        // NOTE: For simplicity, we assume the Cylinder mesh is on the same GameObject as this script.
        // So:
        transform.localScale = newScale;                 // scales the cylinder (height=beamLength)
        transform.localRotation = cylinderRotation;       // rotates cylinder so “height” goes along local Z
        transform.localPosition = localMidpoint;          // moves it forward by half the beam length
    }

    // Draw a red gizmo in the Editor to show the maxDistance
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * MaxDistance);
    }
}
