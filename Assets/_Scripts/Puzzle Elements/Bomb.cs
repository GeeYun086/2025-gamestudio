using System.Collections;
using GravityGame.Gravity;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    [RequireComponent(typeof(Rigidbody))]
    public class Bomb : GravityModifier
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
        [SerializeField] bool _explodeOnGravityChange = true;
        [SerializeField] bool _explodeOnPlayerCollision = true;
        [SerializeField] float _fuseTime = 2f;
        [SerializeField] AudioClip _fuseSound;

        bool _isPrimed;
        AudioSource _audioSource;

        void OnValidate()
        {
            if (_explosionRadius > _pushbackRadius) Debug.LogError($"{name}: Explosion radius should not be larger then pushback radius");
        }

        protected override void Awake()
        {
            base.Awake();

            _audioSource = GetComponent<AudioSource>();
            if (!_audioSource) {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            _audioSource.playOnAwake = false;
        }

        public override Vector3 GravityDirection
        {
            get => base.GravityDirection;
            set {
                if (base.GravityDirection != value) {
                    base.GravityDirection = value;
                    if (_explodeOnGravityChange) PrimeForExplosion();
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (_isPrimed || !_explodeOnPlayerCollision) return;
            if (collision.gameObject.GetComponentInParent<PlayerMovement>()) PrimeForExplosion();
        }

        void PrimeForExplosion()
        {
            if (_isPrimed) return;
            _isPrimed = true;

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
            Instantiate(_explosionVFX, transform.position, Quaternion.identity);
            AudioSource.PlayClipAtPoint(_explosionSound, transform.position, _audioSource.volume);

            var hitColliders = Physics.OverlapSphere(transform.position, _pushback ? Mathf.Max(_explosionRadius, _pushbackRadius) : _explosionRadius);

            PlayerMovement uniquePlayerReference = null;

            foreach (var hitCollider in hitColliders) {
                var pm = hitCollider.GetComponentInParent<PlayerMovement>();
                if (pm) {
                    if (!uniquePlayerReference) {
                        uniquePlayerReference = pm;
                    }
                    continue;
                }

                if (!_pushbackPlayerOnly) {
                    var otherRb = hitCollider.GetComponent<Rigidbody>();
                    if (otherRb) {
                        float distanceToHit = Vector3.Distance(transform.position, hitCollider.transform.position);
                        if (distanceToHit <= _explosionRadius) {
                            otherRb.AddExplosionForce(_playerPushbackForce, transform.position, _explosionRadius, 0f, ForceMode.Impulse);
                        }
                    }
                }
            }

            if (uniquePlayerReference) {
                var playerRigidbody = uniquePlayerReference.GetComponent<Rigidbody>();
                float distanceToPlayer = Vector3.Distance(transform.position, uniquePlayerReference.transform.position);

                if (_killPlayerInRange && distanceToPlayer <= _explosionRadius) {
                    Debug.LogWarning($"{uniquePlayerReference.name} was KILLED by the explosion from {name}!");
                }

                if (playerRigidbody) {
                    if (distanceToPlayer <= _explosionRadius) {
                        playerRigidbody.AddExplosionForce(_playerPushbackForce, transform.position, _explosionRadius, 0f, ForceMode.Impulse);
                    } else if (_pushback && distanceToPlayer <= _pushbackRadius) {
                        playerRigidbody.AddExplosionForce(_playerPushbackForce, transform.position, _pushbackRadius, 0f, ForceMode.Impulse);
                    }
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