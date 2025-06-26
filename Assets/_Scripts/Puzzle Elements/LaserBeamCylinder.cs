using System.Collections.Generic;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.PuzzleElements
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(CapsuleCollider))]
    public class LaserBeamCylinder : MonoBehaviour
    {
        float _maxDistance;
        float _beamRadius;
        float _flatDamage;
        float _knockbackForce;
        float _damageCooldown;
        LayerMask _obstacleMask;
        
        MeshFilter _meshFilter;
        MeshRenderer _meshRenderer;
        CapsuleCollider _collider;

        // Cooldown to avoid spamming the player with damage every frame
        readonly Dictionary<PlayerHealth, float> _cooldowns = new();

        ///public float DamageCooldown = 0.3f;

        public void Configure(ILaserConfig config)
        {
            _maxDistance = config.MaxDistance;
            _beamRadius = config.BeamRadius;
            _flatDamage = config.FlatDamage;
            _knockbackForce = config.KnockbackForce;
            _damageCooldown = config.DamageCooldown;
            _obstacleMask = config.ObstacleMask;
            UpdateBeamAndCollider();
        }


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

        //void OnCollisionEnter(Collision collision)
        //{
        //    TryDamageAndKnockback(collision.collider);
        //}

        void OnCollisionStay(Collision collision)
        {
            TryDamageAndKnockback(collision.collider);
        }

        void TryDamageAndKnockback(Collider other)
        {
            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth == null) return;

            // Cooldown check
            if (_cooldowns.TryGetValue(playerHealth, out float lastTime) &&
                Time.time - lastTime < _damageCooldown)
                return;

            playerHealth.TakeDamage(_flatDamage);
            Debug.Log($"[LaserBeamCylinder] Player hit by collision! Damage applied: {_flatDamage}. Current Health: {playerHealth.CurrentHealth}");

            var playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null) {
                // Push away from the center of the beam
                var knockbackDir = (other.transform.position - transform.position).normalized;
                knockbackDir.y = 0f; // Keep it horizontal
                playerRb.AddForce(knockbackDir * _knockbackForce, ForceMode.Impulse);
                Debug.Log($"[LaserBeamCylinder] Knockback applied to player: {knockbackDir * _knockbackForce}");
            }

            if (playerHealth.CurrentHealth <= 0)
                Debug.Log("[LaserBeamCylinder] Player killed by laser.");

            // Subscribe to death event
            playerHealth.OnPlayerDied.RemoveListener(LogPlayerDied);
            playerHealth.OnPlayerDied.AddListener(LogPlayerDied);

            _cooldowns[playerHealth] = Time.time;
        }

        void UpdateBeamAndCollider()
        {
            var origin = transform.parent;
            if (origin == null)
                return;

            var start = origin.position + origin.forward * 0.01f;
            var dir = origin.forward;
            float length = _maxDistance;

            if (Physics.Raycast(start, dir, out var hit, _maxDistance, _obstacleMask)) {
                length = hit.distance;
            }

            // Always align and scale the laser cylinder forward
            transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            transform.localScale = new Vector3(_beamRadius, length * 0.5f, _beamRadius);
            transform.localPosition = new Vector3(0f, 0f, length * 0.5f);

            // Update collider to match visuals, compensating for transform scaling
            _collider.direction = 1; // Y-axis
            _collider.radius = _beamRadius;

            // Inverse of the Y-scale so that height * scaleY == world-space length
            float invScaleY = 1f / transform.localScale.y;
            float unclampedH = length * invScaleY;
            _collider.height = Mathf.Max(0.01f, unclampedH);

            // Center half-way along the beam in world space, then unscale
            float unclampedCz = length * 0.5f * invScaleY;
            _collider.center = new Vector3(0f, 0f, unclampedCz);
        }

        void LogPlayerDied()
        {
            Debug.Log("[LaserBeamCylinder] Player death event received (OnPlayerDied).");
        }
    }
}