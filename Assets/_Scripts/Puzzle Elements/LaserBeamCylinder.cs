using UnityEngine;
using GravityGame.Player;

namespace GravityGame.PuzzleElements
{
    /// <summary>
    /// Renders a laser beam that damages, knocks back, and clamps the player.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class LaserBeamCylinder : MonoBehaviour
    {
        [Header("Beam Settings")]
        [Tooltip("Max length if nothing blocks the beam.")]
        public float MaxDistance = 20f;
        [Tooltip("Layers that stop the beam (e.g. Default, Cube).")]
        public LayerMask ObstacleMask;

        [Header("Damage Settings")]
        [Tooltip("Flat damage to apply when the player is hit.")]
        public float FlatDamage = 80f;
        [Tooltip("Seconds between damage ticks.")]
        public float HitCooldown = 0.5f;

        [Header("Knockback Settings")]
        [Tooltip("Instantaneous force applied to the player on hit.")]
        public float KnockbackForce = 5f;

        [Header("Visual Settings")]
        [Tooltip("Radius of the beam (half its diameter).")]
        public float BeamRadius = 0.05f;

        private float _lastHitTime = -Mathf.Infinity;

        void Update()
        {
            // Visual beam computation
            Transform origin = transform.parent != null ? transform.parent : transform;
            Vector3 start = origin.position + origin.forward * 0.01f;
            Vector3 dir   = origin.forward;

            float length = MaxDistance;
            if (Physics.Raycast(start, dir, out var obstacleHit, MaxDistance, ObstacleMask))
                length = obstacleHit.distance;

            transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            transform.localScale    = new Vector3(BeamRadius, length * 0.5f, BeamRadius);
            transform.localPosition = new Vector3(0f, 0f, length * 0.5f);
        }

        void LateUpdate()
        {
            // Damage & knockback after all movement
            Transform origin = transform.parent != null ? transform.parent : transform;
            Vector3 start = origin.position + origin.forward * 0.01f;
            Vector3 dir   = origin.forward;

            if (Time.time - _lastHitTime < HitCooldown)
                return;

            int playerMask = 1 << LayerMask.NameToLayer("Player");
            if (!Physics.Raycast(start, dir, out var hit, transform.localScale.y * 2f, playerMask))
                return;

            if (!hit.collider.CompareTag("Player"))
                return;

            _lastHitTime = Time.time;

            // Apply damage
            if (hit.collider.TryGetComponent<PlayerHealth>(out var ph))
                ph.TakeDamage(FlatDamage);

            // Compute safe position just outside the beam
            Vector3 safePos = hit.point - dir * (BeamRadius + 0.01f);

            // Strong knockback + clamp
            if (hit.collider.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(-dir * KnockbackForce, ForceMode.VelocityChange);
                rb.position = safePos;
            }
            else if (hit.collider.TryGetComponent<CharacterController>(out var cc))
            {
                cc.enabled = false;
                cc.transform.position = safePos;
                cc.enabled = true;
            }
        }

        void OnDrawGizmosSelected()
        {
            Transform origin = transform.parent != null ? transform.parent : transform;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin.position, origin.position + origin.forward * MaxDistance);
        }
    }
}
