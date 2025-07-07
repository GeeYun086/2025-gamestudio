using System.Collections.Generic;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.PuzzleElements
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(CapsuleCollider))]
    public class LaserBeamCylinder : MonoBehaviour
    {
        LaserSpawner _laserSpawner;
        MeshFilter _meshFilter;
        MeshRenderer _meshRenderer;
        CapsuleCollider _collider;

        // Cooldown to avoid spamming the player with damage every frame
        readonly Dictionary<PlayerHealth, float> _cooldowns = new();
        
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
            _laserSpawner = GetComponentInParent<LaserSpawner>();
            UpdateBeamAndCollider();
        }

        void OnEnable() => UpdateBeamAndCollider();
        void FixedUpdate() => UpdateBeamAndCollider();

        void OnCollisionEnter(Collision collision)
        {
            TryDamageAndKnockback(collision);
        }

        void OnCollisionStay(Collision collision)
        {
            TryDamageAndKnockback(collision);
        }

        /// <summary>
        /// Carl:
        /// The knockback is no longer based on world coordinates but on the local coordinates of the laser. This should lead to equal behaviour
        /// regardless of the player's gravity.
        /// </summary>
        void TryDamageAndKnockback(Collision other)
        {
            var playerHealth = other.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth == null) return;

            // Cooldown check
            if (_cooldowns.TryGetValue(playerHealth, out float lastTime) &&
                Time.time - lastTime < _laserSpawner.DamageCooldown)
                return;

            playerHealth.TakeDamage(_laserSpawner.FlatDamage);
            Debug.Log($"[LaserBeamCylinder] Player hit by collision! Damage applied: {_laserSpawner.FlatDamage}. Current Health: {playerHealth.CurrentHealth}");

            var playerRb = other.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null) {
                //playerRb.linearVelocity = Vector3.zero;
                Vector3 localPlayerPos = transform.InverseTransformPoint(other.transform.position);
                Vector3 localPushDir = (localPlayerPos-new Vector3(0,localPlayerPos.y,0)).normalized;
                Vector3 worldPushDir = transform.TransformDirection(localPushDir);
                float totalForce = _laserSpawner.KnockbackForce;
                playerRb.AddForce(worldPushDir * totalForce, ForceMode.Impulse);
                Debug.Log($"[LaserBeamCylinder] Knockback applied to player: {worldPushDir * _laserSpawner.KnockbackForce}");
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
            float length = _laserSpawner.MaxDistance;

            if (Physics.Raycast(start, dir, out var hit, _laserSpawner.MaxDistance, _laserSpawner.ObstacleMask)) {
                length = hit.distance;
            }

            // Always align and scale the laser cylinder forward
            transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            transform.localScale = new Vector3(_laserSpawner.BeamRadius, length * 0.5f, _laserSpawner.BeamRadius);
            transform.localPosition = new Vector3(0f, 0f, length * 0.5f);
            
            // Update collider to match visuals, compensating for transform scaling
            _collider.direction = 1; // Y-axis
            _collider.radius = _laserSpawner.BeamRadius;

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