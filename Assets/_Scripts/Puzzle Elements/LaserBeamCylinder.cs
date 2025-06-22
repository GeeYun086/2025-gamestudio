using UnityEngine;
using GravityGame.Player;  // for PlayerHealth

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// A cylinder‐based laser beam that can be toggled on/off via RedstoneComponent.
    /// When powered it scales/positions its mesh, damages the player, and strongly clamps them outside the beam.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class LaserBeamCylinder : RedstoneComponent
    {
        [Header("Beam Settings")]
        [Tooltip("Max distance of the beam if nothing blocks it.")]
        public float MaxDistance = 20f;
        [Tooltip("Which layers physically block the beam (e.g. Default, Cube).")]
        public LayerMask ObstacleMask;

        [Header("Damage Settings")]
        [Tooltip("Flat damage applied on each hit.")]
        public float FlatDamage = 80f;
        [Tooltip("Minimum seconds between damage ticks.")]
        public float HitCooldown = 0.5f;

        [Header("Knockback Settings")]
        [Tooltip("Instantaneous force to push the player out of the beam.")]
        public float KnockbackForce = 5f;

        [Header("Visual Settings")]
        [Tooltip("Radius of the beam (half diameter).")]
        public float BeamRadius = 0.05f;

        MeshRenderer _mesh;
        bool         _isPowered;
        float        _lastHitTime = -Mathf.Infinity;

        /// <summary>
        /// Called by Redstone logic. Turning on shows + activates the beam; off hides + disables it.
        /// </summary>
        public override bool IsPowered
        {
            get => _isPowered;
            set
            {
                if (_isPowered == value) return;
                _isPowered = value;
                _mesh.enabled = _isPowered;
            }
        }

        void Awake()
        {
            _mesh = GetComponent<MeshRenderer>();
            _mesh.enabled = false;  // hidden until powered
        }

        void Update()
        {
            if (!IsPowered) return;

            // Compute origin (parent if present, else self)
            Transform origin = transform.parent != null ? transform.parent : transform;
            Vector3 start    = origin.position + origin.forward * 0.01f;
            Vector3 dir      = origin.forward;

            // Raycast obstacle
            float length = MaxDistance;
            if (Physics.Raycast(start, dir, out var obsHit, MaxDistance, ObstacleMask))
                length = obsHit.distance;

            // Visual: rotate, scale, position cylinder
            transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            transform.localScale    = new Vector3(BeamRadius, length * 0.5f, BeamRadius);
            transform.localPosition = new Vector3(0f, 0f, length * 0.5f);
        }

        void LateUpdate()
        {
            if (!IsPowered || Time.time - _lastHitTime < HitCooldown)
                return;

            Transform origin = transform.parent != null ? transform.parent : transform;
            Vector3 start    = origin.position + origin.forward * 0.01f;
            Vector3 dir      = origin.forward;

            // Only hit the Player layer
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            if (!Physics.Raycast(start, dir, out var hit, transform.localScale.y * 2f, playerMask))
                return;
            if (!hit.collider.CompareTag("Player"))
                return;

            _lastHitTime = Time.time;

            // Damage
            if (hit.collider.TryGetComponent<PlayerHealth>(out var ph))
                ph.TakeDamage(FlatDamage);

            // Compute safe position behind the beam surface
            Vector3 safePos = hit.point - dir * (BeamRadius + 0.01f);

            // Clamp + strong knockback
            if (hit.collider.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.MovePosition(safePos);
                rb.AddForce(-dir * KnockbackForce, ForceMode.VelocityChange);
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
