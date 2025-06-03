using System.Collections;
using GravityGame.Gravity;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Represents a bomb that will be armed upon its trigger being entered.
    /// After a configurable duration <see cref="_fuseTime"/>, it detonates,
    /// either killing or pushing back the player, depending on the configured radii.
    /// Also pushes other physics objects, kills enemies, and breaks destructibles.
    /// </summary>
    [RequireComponent(typeof(GravityModifier))]
    [RequireComponent(typeof(Rigidbody))]
    public class Bomb : MonoBehaviour
    {
        [Header("Explosion")]
        [SerializeField] float _explosionRadius = 5f;
        [SerializeField] GameObject _explosionVFX;
        [SerializeField] AudioClip _explosionSound;
        [SerializeField] bool _enableExplosionDamage = true;

        [Header("Pushback")]
        [SerializeField] bool _pushback = true;
        [SerializeField] float _pushbackRadius = 8.5f;
        [SerializeField] float _pushbackForce = 10000f;

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
            if (!_audioSource) _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        void OnTriggerEnter(Collider other)
        {
            if (_isArmed) return;
            if (other.gameObject == gameObject || other.transform.IsChildOf(transform)) return;
            if (GetComponent<Rigidbody>() && other.attachedRigidbody == GetComponent<Rigidbody>()) return;
            ArmForExplosion();
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
            if (_fuseSound && _audioSource) _audioSource.PlayOneShot(_fuseSound);
            yield return new WaitForSeconds(_fuseTime);
            Detonate();
        }

        void Detonate()
        {
            if (_explosionVFX) Instantiate(_explosionVFX, transform.position, Quaternion.identity);
            if (_explosionSound && _audioSource) AudioSource.PlayClipAtPoint(_explosionSound, transform.position, _audioSource.volume);

            PlayerMovement playerRef = null;

            float overlapRadius = _pushback ? Mathf.Max(_explosionRadius, _pushbackRadius) : _explosionRadius;
            var hitColliders = new Collider[100];
            int colliders = Physics.OverlapSphereNonAlloc(transform.position, overlapRadius, hitColliders);

            for (int i = 0; i < colliders; i++) {
                if (!hitColliders[i] || hitColliders[i].transform.IsChildOf(transform) || hitColliders[i].gameObject == gameObject) continue;

                var playerComponent = hitColliders[i].GetComponentInParent<PlayerMovement>();
                if (playerComponent) {
                    if (!playerRef) playerRef = playerComponent;
                    continue;
                }

                float distance = Vector3.Distance(transform.position, hitColliders[i].transform.position);

                var breakableComponent = hitColliders[i].GetComponentInParent<Breakable>();
                if (breakableComponent) {
                    if (distance <= _explosionRadius) {
                        breakableComponent.Break();
                        continue;
                    }
                }

                var enemyComponent = hitColliders[i].GetComponentInParent<NavMeshPatrol>();
                if (enemyComponent) {
                    if (distance <= _explosionRadius) {
                        var enemyRb = enemyComponent.GetComponentInParent<Rigidbody>();
                        if (enemyRb) {
                            enemyRb.AddExplosionForce(_pushbackForce, transform.position, _explosionRadius, 0f, ForceMode.Impulse);
                        }
                        Destroy(enemyComponent.gameObject);
                        continue;
                    }
                }

                var rb = hitColliders[i].GetComponentInParent<Rigidbody>();
                if (rb) {
                    if (distance <= _explosionRadius) {
                        rb.AddExplosionForce(_pushbackForce, transform.position, _explosionRadius, 0f, ForceMode.Impulse);
                    } else if (_pushback && distance <= _pushbackRadius) {
                        rb.AddExplosionForce(_pushbackForce, transform.position, _pushbackRadius, 0f, ForceMode.Impulse);
                    }
                }
            }

            if (playerRef) {
                var playerRb = playerRef.GetComponent<Rigidbody>();
                float distanceToPlayer = Vector3.Distance(transform.position, playerRef.transform.position);

                if (distanceToPlayer <= _explosionRadius) {
                    if (_enableExplosionDamage) PlayerHealth.Instance.TakeDamage(PlayerHealth.MaxHealth * 2f);
                    if (playerRb) playerRb.AddExplosionForce(_pushbackForce, transform.position, _explosionRadius, 0f, ForceMode.Impulse);
                } else if (_pushback && distanceToPlayer <= _pushbackRadius) {
                    if (playerRb) playerRb.AddExplosionForce(_pushbackForce, transform.position, _pushbackRadius, 0f, ForceMode.Impulse);
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