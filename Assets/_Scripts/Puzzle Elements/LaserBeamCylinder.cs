using System.Collections.Generic;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.PuzzleElements
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(CapsuleCollider))]
    public class LaserBeamCylinder : MonoBehaviour
    {
        [Header("Beam Settings")]
        public float MaxDistance = 20f;
        public LayerMask ObstacleMask;

        [Header("Damage Settings")]
        public float FlatDamage = 80f;

        [Header("Visual Settings")]
        public float BeamRadius = 0.1f;

        MeshFilter _meshFilter;
        MeshRenderer _meshRenderer;
        CapsuleCollider _collider;

        // Cooldown to avoid spamming damage every frame
        Dictionary<PlayerHealth, float> _cooldowns = new();
        public float DamageCooldown = 0.3f;

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

        void Start() => UpdateBeamAndCollider();
        void OnEnable() => UpdateBeamAndCollider();
        void Update() => UpdateBeamAndCollider();

        void OnCollisionEnter(Collision col) => TryDamage(col.collider);
        void OnCollisionStay(Collision col) => TryDamage(col.collider);

        void TryDamage(Collider other)
        {
            // Guard against destroyed collider
            if (other == null || other.gameObject == null) return;

            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth == null) return;

            // Clean out any dead entries
            if (_cooldowns.ContainsKey(playerHealth) && playerHealth == null) {
                _cooldowns.Remove(playerHealth);
                return;
            }

            // Cooldown check
            if (_cooldowns.TryGetValue(playerHealth, out float lastTime)
                && Time.time - lastTime < DamageCooldown)
                return;

            // Deal damage
            playerHealth.TakeDamage(FlatDamage);
            Debug.Log($"[LaserBeamCylinder] Player hit! Damage: {FlatDamage}. Health now: {playerHealth.CurrentHealth}");

            // If they died, log it and remove from cooldowns
            if (playerHealth.CurrentHealth <= 0) {
                Debug.Log("[LaserBeamCylinder] Player killed by laser.");
                _cooldowns.Remove(playerHealth);
                return;
            }

            // Reset cooldown
            _cooldowns[playerHealth] = Time.time;
        }

        void UpdateBeamAndCollider()
        {
            var origin = transform.parent;
            if (origin == null) return;

            var start = origin.position + origin.forward * 0.01f;
            var dir = origin.forward;
            float length = MaxDistance;

            if (Physics.Raycast(start, dir, out var hit, MaxDistance, ObstacleMask))
                length = hit.distance;

            // Visual scaling
            transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            transform.localScale = new Vector3(BeamRadius, length * 0.5f, BeamRadius);
            transform.localPosition = new Vector3(0f, 0f, length * 0.5f);

            // Collider: un‐scale height & center so it matches world‐space length
            _collider.direction = 1; // Y‐axis
            _collider.radius = BeamRadius;
            float invY = 1f / transform.localScale.y;
            _collider.height = Mathf.Max(0.01f, length * invY);
            _collider.center = new Vector3(0f, 0f, length * 0.5f * invY);
        }
    }
}