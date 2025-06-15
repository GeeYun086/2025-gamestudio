using GravityGame.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace GravityGame.PuzzleElements
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class LaserBeamCylinder : MonoBehaviour
    {
        [FormerlySerializedAs("maxDistance")]
        [Header("Beam Settings")]
        [Tooltip("Max length if nothing blocks.")]
        public float MaxDistance = 20f;

        [FormerlySerializedAs("obstacleMask")] [Tooltip("Layers that stop the beam (Default & Cube).")]
        public LayerMask ObstacleMask;

        [FormerlySerializedAs("flatDamage")]
        [Header("Damage Settings")]
        [Tooltip("Flat damage to apply on the first frame of contact.")]
        public float FlatDamage = 80f;

        [FormerlySerializedAs("beamRadius")]
        [Header("Visual Settings")]
        [Tooltip("Cylinder radius.")]
        public float BeamRadius = 0.1f;

        MeshFilter _meshFilter;
        MeshRenderer _meshRenderer;
        CapsuleCollider _collider;
        int _playerMask;
        bool _hasDamagedThisContact;

        void Awake()
        {
            // mesh + material setup
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            if (_meshFilter.sharedMesh == null) {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                _meshFilter.sharedMesh = tmp.GetComponent<MeshFilter>().sharedMesh;
                DestroyImmediate(tmp);
            }
            if (_meshRenderer.sharedMaterial == null) {
                var mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = Color.red;
                _meshRenderer.sharedMaterial = mat;
            }

            // collider setup
            _collider = GetComponent<CapsuleCollider>();
            _collider.isTrigger = false;
            _collider.direction = 2; // Z axis

            // cache mask
            _playerMask = 1 << LayerMask.NameToLayer("Player");
        }

        void Update()
        {
            // 1) Compute beam length via Default & Cube only
            var origin = transform.parent;
            var start = origin.position + origin.forward * 0.01f;
            var dir = origin.forward;

            // draw full‐length in Scene
            Debug.DrawRay(start, dir * MaxDistance, Color.yellow);

            float length = MaxDistance;
            if (Physics.Raycast(start, dir, out var blockHit, MaxDistance, ObstacleMask)) {
                length = blockHit.distance;
            }

            // 2) Raycast for player only within that length
            bool hitPlayer = Physics.Raycast(start, dir, out var phHit, length, _playerMask);

            if (hitPlayer && !_hasDamagedThisContact) {
                // first frame of touching beam → apply damage
                var ph = phHit.collider.GetComponent<PlayerHealth>();
                if (ph != null) {
                    Debug.Log($"[Laser] Hitting Player: dealing {FlatDamage} damage");
                    ph.TakeDamage(FlatDamage);
                }
                _hasDamagedThisContact = true;
            } else if (!hitPlayer) {
                // player has left the beam → reset for next entry
                _hasDamagedThisContact = false;
            }

            // 3) Resize & position the visual cylinder
            transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            transform.localScale = new Vector3(BeamRadius, length * 0.5f, BeamRadius);
            transform.localPosition = new Vector3(0f, 0f, length * 0.5f);

            // 4) Resize & position the capsule collider
            _collider.radius = BeamRadius;
            _collider.height = length;
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
    }
}