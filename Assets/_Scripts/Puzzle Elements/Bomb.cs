using UnityEngine;
using System.Collections;
using GravityGame.Gravity;
using GravityGame.Player;

namespace GravityGame.Puzzle_Elements
{
    [RequireComponent(typeof(Rigidbody))]
    public class Bomb : GravityModifier
    {
        [Header("Explosion Settings")]
        [SerializeField] GameObject _explosionVFXPrefab;
        [SerializeField] float _explosionRadius = 5f;
        [SerializeField] float _explosionForce = 700f;
        [SerializeField] float _upwardsModifier = 0.5f;
        [SerializeField] LayerMask _affectedLayers;

        [Header("Fuse & Sound Settings")]
        [SerializeField] float _fuseTime = 0.5f;
        [SerializeField] AudioClip _fuseSound;
        [SerializeField] AudioClip _explosionSound;

        [Header("Trigger Conditions")]
        [SerializeField] bool _explodeOnGravityChangeAttempt = true;
        [SerializeField] bool _explodeOnPlayerCollision = true;

        bool _isPrimed = false;
        AudioSource _audioSource;

        protected override void Awake()
        {
            base.Awake();
            if (_fuseSound != null || _explosionSound != null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }

        public override Vector3 GravityDirection
        {
            get => base.GravityDirection;
            set
            {
                if (_isPrimed)
                {
                    return;
                }

                if (_explodeOnGravityChangeAttempt)
                {
                    PrimeForExplosion();
                }
                else
                {
                    base.GravityDirection = value;
                }
            }
        }
        
        public void TriggerByPlayerInteraction()
        {
            if (_explodeOnGravityChangeAttempt)
            {
                PrimeForExplosion();
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!_explodeOnPlayerCollision || _isPrimed) return;

            if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerMovement>() != null)
            {
                PrimeForExplosion();
            }
        }

        void PrimeForExplosion()
        {
            if (_isPrimed) return;

            _isPrimed = true;

            if (_fuseTime > 0)
            {
                StartCoroutine(ExplosionSequenceCoroutine());
            }
            else
            {
                Detonate();
            }
        }

        IEnumerator ExplosionSequenceCoroutine()
        {
            if (_audioSource != null && _fuseSound != null)
            {
                _audioSource.PlayOneShot(_fuseSound);
            }
            yield return new WaitForSeconds(_fuseTime);
            Detonate();
        }

        void Detonate()
        {
            if (_explosionVFXPrefab != null)
            {
                Instantiate(_explosionVFXPrefab, transform.position, Quaternion.identity);
            }

            if (_explosionSound != null)
            {
                AudioSource.PlayClipAtPoint(_explosionSound, transform.position, _audioSource != null ? _audioSource.volume : 1.0f);
            }

            Collider[] collidersInRange = Physics.OverlapSphere(transform.position, _explosionRadius, _affectedLayers);
            foreach (Collider hitCollider in collidersInRange)
            {
                Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius, _upwardsModifier, ForceMode.Impulse);
                }
            }
            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}