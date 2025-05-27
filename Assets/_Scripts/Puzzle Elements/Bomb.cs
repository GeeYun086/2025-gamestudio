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
        [SerializeField] bool _killPlayerInRange = true;
        [SerializeField] float _explosionRadius = 5f;
        [SerializeField] GameObject _explosionVFX;
        [SerializeField] AudioClip _explosionSound;

        [Header("Pushback")]
        [SerializeField] bool _pushbackPlayer = true;
        [SerializeField] float _pushbackRadius = 8.5f;

        [Header("Interaction")]
        [SerializeField] bool _explodeOnGravityChange = true;
        [SerializeField] bool _explodeOnPlayerCollision = true;
        [SerializeField] float _fuseTime = 0.5f;
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

            if (_fuseSound || _explosionSound) {
                _audioSource = GetComponent<AudioSource>();
                if (!_audioSource) _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        public override Vector3 GravityDirection
        {
            get => base.GravityDirection;
            set {
                if (_isPrimed) return;

                if (_explodeOnGravityChange) {
                    PrimeForExplosion();
                } else {
                    base.GravityDirection = value;
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!_explodeOnPlayerCollision || _isPrimed) return;

            if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerMovement>()) PrimeForExplosion();
        }

        void PrimeForExplosion()
        {
            if (_isPrimed) return;

            _isPrimed = true;

            if (_fuseTime > 0) {
                StartCoroutine(ExplosionSequenceCoroutine());
            } else {
                Detonate();
            }
        }

        IEnumerator ExplosionSequenceCoroutine()
        {
            if (_audioSource && _fuseSound) _audioSource.PlayOneShot(_fuseSound);
            yield return new WaitForSeconds(_fuseTime);
            Detonate();
        }

        void Detonate()
        {
            if (_explosionVFX) Instantiate(_explosionVFX, transform.position, Quaternion.identity);

            if (_explosionSound) AudioSource.PlayClipAtPoint(_explosionSound, transform.position, _audioSource ? _audioSource.volume : 1.0f);

            var collidersInRange = Physics.OverlapSphere(transform.position, _explosionRadius, 0);
            foreach (var hitCollider in collidersInRange) {
                var rb = hitCollider.GetComponent<Rigidbody>();
                if (rb) rb.AddExplosionForce(1000, transform.position, _explosionRadius, 0, ForceMode.Impulse);
            }
            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);

            Gizmos.color = new Color(0f, 1f, 0.24f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, _pushbackRadius);
        }
    }
}