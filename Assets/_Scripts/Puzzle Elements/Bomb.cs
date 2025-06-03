using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using GravityGame.Gravity;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Represents a bomb that will be armed upon collision.
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
        [SerializeField] bool _killPlayerInRange = true;
        [SerializeField] float _playerDamageAmount = 35f;

        [Header("Player Pushback")]
        [SerializeField] bool _pushback = true;
        [SerializeField] float _pushbackRadius = 8.5f;
        [SerializeField] float _pushbackForce = 1000f;

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
            if (_fuseSound && _audioSource) _audioSource.PlayOneShot(_fuseSound);
            yield return new WaitForSeconds(_fuseTime);
            Detonate();
        }

        void Detonate()
        {
            if (_explosionVFX) Instantiate(_explosionVFX, transform.position, Quaternion.identity);
            if (_explosionSound && _audioSource) AudioSource.PlayClipAtPoint(_explosionSound, transform.position, _audioSource.volume);

            PlayerMovement playerRef = null;
            PlayerHealth playerHealthRef = null;

            float overlapSphereRadiusToScan = _pushback ? Mathf.Max(_explosionRadius, _pushbackRadius) : _explosionRadius;
            var hitColliders = new Collider[100];
            int numActualColliders = Physics.OverlapSphereNonAlloc(transform.position, overlapSphereRadiusToScan, hitColliders);

            for (int i = 0; i < numActualColliders; i++) {
                var hitCollider = hitColliders[i];
                if (!hitCollider || hitCollider.transform.IsChildOf(transform) || hitCollider.gameObject == gameObject) continue;

                var playerComponent = hitCollider.GetComponentInParent<PlayerMovement>();
                if (playerComponent) {
                    if (!playerRef) {
                        playerRef = playerComponent;
                        playerHealthRef = playerRef.GetComponent<PlayerHealth>();
                    }
                    continue;
                }

                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);

                var breakableComponent = hitCollider.GetComponentInParent<Breakable>();
                if (breakableComponent) {
                    if (distance <= _explosionRadius) {
                        breakableComponent.Break();
                        continue; 
                    }
                }
                
                var enemyComponent = hitCollider.GetComponentInParent<NavMeshPatrol>();
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

                var rb = hitCollider.GetComponentInParent<Rigidbody>();
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
                    if (playerHealthRef) {
                        if (_killPlayerInRange) {
                            playerHealthRef.TakeDamage(PlayerHealth.MaxHealth * 2f);
                        } else {
                            playerHealthRef.TakeDamage(_playerDamageAmount);
                        }
                    }
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