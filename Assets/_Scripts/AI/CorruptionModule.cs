using System.Collections;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace GravityGame
{
    /// <summary>
    ///     Ignores player on far distance, gets provoked on middle distance and shoots after 2s. Gets aggressive on short distance and shoots after .5s.
    /// </summary>
    public class CorruptionModule : MonoBehaviour
    {
        Transform _player;
        Renderer _sphereRenderer;
        GameObject _laser;
        GameObject _fakeLaser;
        [SerializeField] Material _unprovoked;
        [SerializeField] Material _provoked;
        [SerializeField] Material _aggressive;
        Stopwatch _timer;
        Vector3? _targetForward;

        void Awake()
        {
            _sphereRenderer = gameObject.transform.GetChild(0).GetComponent<Renderer>();
            _laser = gameObject.transform.GetChild(0).GetChild(3).gameObject;
            _fakeLaser = gameObject.transform.GetChild(0).GetChild(4).gameObject;
            _player = GameObject.FindWithTag("Player").transform;
        }

        void Update()
        {
            if (!transform.parent) {
                _laser.SetActive(false);
                _fakeLaser.SetActive(false);
                return;
            }
            float distance = Vector3.Distance(_player.position, transform.position);
            switch (distance) {
                case < 8:
                    _sphereRenderer.material = _provoked;
                    _fakeLaser.SetActive(true);
                    Shoot(.5f);
                    break;
                case < 12:
                    _sphereRenderer.material = _aggressive;
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
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 60f);
        }

        void Shoot(float delay)
        {
            Vector3 playerHead = _player.position + new Vector3(0, 1f, 0);
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
    }
}