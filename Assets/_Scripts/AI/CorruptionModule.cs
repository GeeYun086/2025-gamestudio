using System.Collections;
using UnityEngine;
using System.Diagnostics;

namespace GravityGame
{
    /// <summary>
    ///     Ignores player on far distance, gets provoked on middle distance and shoots after 2s. Gets aggressive on short distance and shoots after .5s.
    /// </summary>
    public class CorruptionModule : MonoBehaviour
    {
        Transform _player;
        Renderer _sphereRenderer;
        [SerializeField] GameObject _laser;
        [SerializeField] GameObject _fakeLaser;
        [SerializeField] Material _broken;
        [SerializeField] Material _unprovoked;
        [SerializeField] Material _provoked;
        [SerializeField] Material _aggressive;
        Stopwatch _timer;
        Vector3? _targetForward;
        bool _isBroken;

        void Awake()
        {
            _sphereRenderer = gameObject.transform.GetComponent<Renderer>();
            _player = GameObject.FindWithTag("Player").transform;
            ResetTimer();
        }

        void Update()
        {
            if (_isBroken) return;
            if (!transform.parent) {
                _laser.SetActive(false);
                _fakeLaser.SetActive(false);
                _sphereRenderer.material = _broken;
                GetComponent<Rigidbody>().useGravity = true;
                StartCoroutine(Despawn());
                _isBroken = true;
                return;
            }
            float distance = Vector3.Distance(_player.position, transform.position);
            switch (distance) {
                case < 5.5f:
                    _sphereRenderer.material = _aggressive;
                    _fakeLaser.SetActive(true);
                    Shoot(.5f);
                    break;
                case < 7:
                    _sphereRenderer.material = _provoked;
                    _fakeLaser.SetActive(true);
                    Shoot(2f);
                    break;
                default:
                    _laser.SetActive(false);
                    _fakeLaser.SetActive(false);
                    _sphereRenderer.material = _unprovoked;
                    ResetTimer();
                    LookAround();
                    break;
            }
        }

        void ResetTimer()
        {
            _timer = new Stopwatch();
            _timer.Start();
        }

        void LookAround()
        {
            if (_targetForward == null || Vector3.Angle(transform.forward, _targetForward.Value) < 5f) {
                Vector3 randomDirection = Random.onUnitSphere;
                if (randomDirection.y < 0)
                    randomDirection.y = -randomDirection.y;
                _targetForward = randomDirection;
            }

            Quaternion targetRotation = Quaternion.LookRotation(_targetForward.Value, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 160f);
        }

        void Shoot(float delay)
        {
            Vector3 playerHead = _player.position + _player.transform.up;
            transform.LookAt(playerHead);
            if (_timer.ElapsedMilliseconds / 1000f > delay) {
                _laser.SetActive(true);
                StartCoroutine(DeactivateLaser());
                ResetTimer();
            }
        }

        IEnumerator DeactivateLaser()
        {
            yield return new WaitForSeconds(.3f);
            _laser.SetActive(false);
        }

        IEnumerator Despawn()
        {
            yield return new WaitForSeconds(5f);
            Destroy(gameObject);
        }
    }
}