using System.Collections;
using System.Linq;
using GravityGame.Gravity;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Represents a bomb that will be armed upon collision.
    /// After a configurable duration <see cref="_fuseTime"/>, it detonates,
    /// either killing or pushing back the player, depending on the configured radii.
    /// </summary>
    [RequireComponent(typeof(GravityModifier))]
    [RequireComponent(typeof(Rigidbody))]
    public class Bomb : MonoBehaviour
    {
        [Header("Explosion")]
        [SerializeField] float _explosionRadius = 5f;
        [SerializeField] GameObject _explosionVFX;
        [SerializeField] AudioClip _explosionSound;
        [SerializeField] bool _killPlayerInRange = true;

        [Header("Player Pushback")]
        [SerializeField] bool _pushback = true;
        [SerializeField] bool _pushbackPlayerOnly = true;
        [SerializeField] float _pushbackRadius = 8.5f;
        [SerializeField] float _playerPushbackForce = 1000f;

        [Header("Interaction")]
        [SerializeField] float _fuseTime = 5f;
        [SerializeField] AudioClip _fuseSound;

        bool _isArmed;
        AudioSource _audioSource;

        void OnValidate()
        {
            if (_explosionRadius > _pushbackRadius) Debug.LogError($"{name}: Explosion radius should not be larger then pushback radius");
        }

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (!_audioSource) {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            _audioSource.playOnAwake = false;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (_isArmed) return;
            if (collision.transform.IsChildOf(transform) || collision.gameObject == gameObject) return;
            if (collision.transform.TryGetComponent(out CubeSpawner cubeSpawner)) return;

            if (collision.contacts.Select(contact => contact.thisCollider.transform)
                .Any(contactColliderTransform => contactColliderTransform.parent == transform)) ArmForExplosion();
        }

        void ArmForExplosion()
        {
            if (_isArmed) return;
            _isArmed = true;

            if (_fuseTime > 0f) {
                StartCoroutine(ExplosionSequenceCoroutine());
            } else {
                Detonate();
            }
        }

        IEnumerator ExplosionSequenceCoroutine()
        {
            _audioSource.PlayOneShot(_fuseSound);
            yield return new WaitForSeconds(_fuseTime);
            Detonate();
        }

        void Detonate()
        {
            if (_explosionVFX) Instantiate(_explosionVFX, transform.position, Quaternion.identity);
            if (_explosionSound) AudioSource.PlayClipAtPoint(_explosionSound, transform.position, _audioSource.volume);

            PlayerMovement playerRef = null;

            float overlapRadius = _pushback ? Mathf.Max(_explosionRadius, _pushbackRadius) : _explosionRadius;
            var hitColliders = new Collider[100];
            int colliders = Physics.OverlapSphereNonAlloc(transform.position, overlapRadius, hitColliders);

            for (int i = 0; i < colliders; i++) {
                if (!hitColliders[i]) continue;

                if (hitColliders[i].GetComponentInParent<PlayerMovement>()) {
                    if (!playerRef) playerRef = hitColliders[i].GetComponentInParent<PlayerMovement>();
                    continue;
                }

                if (!_pushbackPlayerOnly) {
                    if (hitColliders[i].GetComponent<Rigidbody>()) {
                        if (Vector3.Distance(transform.position, hitColliders[i].transform.position) <= _explosionRadius)
                            hitColliders[i].GetComponent<Rigidbody>().AddExplosionForce(
                                _playerPushbackForce, transform.position, _explosionRadius, 0f, ForceMode.Impulse
                            );
                    }
                }
            }

            if (playerRef) {
                var playerRb = playerRef.GetComponent<Rigidbody>();
                float distanceToPlayer = Vector3.Distance(transform.position, playerRef.transform.position);

                if (_killPlayerInRange && distanceToPlayer <= _explosionRadius) {
                    // TODO FS: Add kill logic when player health is implemented
                    Debug.LogWarning($"{playerRef.name} killed by {name}");
                }

                if (distanceToPlayer <= _explosionRadius) {
                    playerRb.AddExplosionForce(_playerPushbackForce, transform.position, _explosionRadius, 0f, ForceMode.Impulse);
                } else if (_pushback && distanceToPlayer <= _pushbackRadius) {
                    playerRb.AddExplosionForce(_playerPushbackForce, transform.position, _pushbackRadius, 0f, ForceMode.Impulse);
                }
            }

            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
            Gizmos.DrawSphere(transform.position, _explosionRadius);

            Gizmos.color = new Color(0f, 1f, 0.25f, 0.25f);
            Gizmos.DrawSphere(transform.position, _pushbackRadius);
        }
    }
}