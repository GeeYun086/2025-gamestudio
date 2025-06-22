using GravityGame.Player;
using UnityEngine;

namespace GravityGame.PuzzleElements
{
    /// <summary>
    ///     Renders and controls a cylindrical laser beam: casts a ray each frame up to MaxDistance,
    ///     updates its mesh and collider to match the hit distance, and applies FlatDamage to the player on first contact.
    ///     Now ensures collider is always correct on enable and start, and supports robust prefab instantiation.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(CapsuleCollider))]
    public class LaserBeamCylinder : MonoBehaviour
    {
        [Header("Beam Settings")]
        [Tooltip("Max length if nothing blocks.")]
        public float MaxDistance = 20f;

        [Tooltip("Layers that stop the beam (Default, Cube, Player, etc).")]
        public LayerMask ObstacleMask;

        [Header("Damage Settings")]
        [Tooltip("Flat damage to apply on the first frame of contact.")]
        public float FlatDamage = 80f;

        [Header("Visual Settings")]
        [Tooltip("Cylinder radius.")]
        public float BeamRadius = 0.1f;

        MeshFilter _meshFilter;
        MeshRenderer _meshRenderer;
        CapsuleCollider _collider;
        bool _hasDamagedThisContact;

        void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _collider = GetComponent<CapsuleCollider>();

#if UNITY_EDITOR
            if (_meshFilter.sharedMesh == null)
                Debug.LogError($"[{nameof(LaserBeamCylinder)}] Missing MeshFilter.sharedMesh", this);
            if (_meshRenderer.sharedMaterial == null)
                Debug.LogError($"[{nameof(LaserBeamCylinder)}] Missing MeshRenderer.sharedMaterial", this);
#endif
        }

        void Start()
        {
            ForceUpdateBeam();
        }

        void OnEnable()
        {
            ForceUpdateBeam();
        }

        void Update()
        {
            UpdateBeamAndCollider();
        }

        /// <summary>
        /// Ensures the collider and visuals are correct on enable/spawn even before first Update().
        /// </summary>
        void ForceUpdateBeam()
        {
            UpdateBeamAndCollider();
        }

        void UpdateBeamAndCollider()
        {
            var origin = transform.parent;
            if (origin == null) return;
            var start = origin.position + origin.forward * 0.01f;
            var dir = origin.forward;

            Debug.DrawRay(start, dir * MaxDistance, Color.yellow);

            float length = MaxDistance;

            // Raycast to detect hit
            if (Physics.Raycast(start, dir, out var hit, MaxDistance, ObstacleMask)) {
                length = hit.distance;

                // Did we hit the player?
                var playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null) {
                    if (!_hasDamagedThisContact) {
                        Debug.Log($"[LaserBeam] Hitting Player: dealing {FlatDamage} damage");
                        playerHealth.TakeDamage(FlatDamage);
                        _hasDamagedThisContact = true;
                    }
                } else {
                    // Hit something else → reset damage flag
                    _hasDamagedThisContact = false;
                }
            } else {
                // Nothing in the way → full-length beam, clear damage state
                _hasDamagedThisContact = false;
            }

            // --- Update visuals & collider to the computed length ---
            transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            transform.localScale = new Vector3(BeamRadius, length * 0.5f, BeamRadius);
            transform.localPosition = new Vector3(0f, 0f, length * 0.5f);

            // Collider setup (reliable even if beam length is very short)
            _collider.direction = 1; // Y-axis
            _collider.radius = BeamRadius;
            _collider.height = Mathf.Max(0.01f, length);
            _collider.center = new Vector3(0f, 0f, length * 0.5f);
        }

        void OnDrawGizmosSelected()
        {
            if (transform.parent == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                transform.parent.position,
                transform.parent.position + transform.parent.forward * MaxDistance
            );
        }

        // Optional: Visualize collider (Editor only)
        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (_collider == null) _collider = GetComponent<CapsuleCollider>();
            if (_collider == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            float halfHeight = (_collider.height * 0.5f) - _collider.radius;
            Vector3 up = Vector3.up * halfHeight;
            Gizmos.DrawWireSphere(_collider.center + up, _collider.radius);
            Gizmos.DrawWireSphere(_collider.center - up, _collider.radius);
#endif
        }
    }
}
