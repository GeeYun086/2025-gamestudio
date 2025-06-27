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
        [SerializeField] Material _unprovoked;
        [SerializeField] Material _provoked;
        [SerializeField] Material _aggressive;
        Stopwatch _timer;
        Vector3? _targetRotation;

        void Awake()
        {
            _sphereRenderer = gameObject.transform.GetChild(0).GetComponent<Renderer>();
            _laser = gameObject.transform.GetChild(0).GetChild(3).gameObject;
            _player = GameObject.FindWithTag("Player").transform;
        }

        void Update()
        {
            float distance = Vector3.Distance(_player.position, transform.position);
            switch (distance) {
                case < 8:
                    _sphereRenderer.material = _provoked;
                    Shoot(.5f);
                    break;
                case < 12:
                    _sphereRenderer.material = _aggressive;
                    Shoot(2f);
                    break;
                default:
                    _laser.SetActive(false);
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
            if (_targetRotation == null || (transform.localEulerAngles - _targetRotation.Value).magnitude < 60f)
                _targetRotation = new Vector3(Random.Range(-85, 0), Random.Range(-180, 180), 0);
            if (_targetRotation != null)
                transform.Rotate((transform.rotation.eulerAngles - _targetRotation.Value) * Time.deltaTime / 3);
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