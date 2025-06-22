using GravityGame.Player;
using UnityEngine;

namespace GravityGame.PuzzleElements
{
    /// <summary>
    ///     Renders and controls a cylindrical laser beam: casts a ray each frame up to MaxDistance,
    ///     updates its mesh and collider to match the hit distance, and applies FlatDamage to the player on first contact.
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
            UpdateBeamAndCollider();
        }

        void OnEnable()
        {
            UpdateBeamAndCollider();
        }

        void Update()
        {
            UpdateBeamAndCollider();
        }

        /// <summary>
        ///     Updates the beam's visuals and collider based on the raycast result.
        /// </summary>
        void UpdateBeamAndCollider()
        {
            var origin = transform.parent;
            if (origin == null)
                return;

            var start = origin.position + origin.forward * 0.01f;
            var dir = origin.forward;
            float length = MaxDistance;

            // Raycast to find hit point
            if (Physics.Raycast(start, dir, out var hit, MaxDistance, ObstacleMask)) {
                length = hit.distance;

                // Apply damage if the player was hit
                var playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null) {
                    if (!_hasDamagedThisContact) {
                        playerHealth.TakeDamage(FlatDamage);
                        _hasDamagedThisContact = true;
                    }
                } else {
                    _hasDamagedThisContact = false;
                }
            } else {
                _hasDamagedThisContact = false;
            }

            // Always align and scale the laser cylinder forward
            transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            transform.localScale = new Vector3(BeamRadius, length * 0.5f, BeamRadius);
            transform.localPosition = new Vector3(0f, 0f, length * 0.5f);

            // Update collider to match visuals
            _collider.direction = 1; // Y-axis
            _collider.radius = BeamRadius;
            _collider.height = Mathf.Max(0.01f, length);
            _collider.center = new Vector3(0f, 0f, length * 0.5f);
        }
    }
}