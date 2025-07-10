using System.Collections;
using GravityGame.SaveAndLoadSystem;
using GravityGame.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace GravityGame.Player
{
    /// <summary>
    ///     Manages the player's health, including taking damage, healing, and death.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerHealth : SingletonMonoBehavior<PlayerHealth>
    {
        [SerializeField] PlayerMovement _playerMovement;
        [SerializeField] FirstPersonCameraController _cameraController;
        [SerializeField] SmoothCamera _smoothCamera;
        [SerializeField] Transform _cameraTransform;

        public static float MaxHealth => 100f;
        public float CurrentHealth { get; private set; }
        bool IsDead => CurrentHealth <= 0;

        const float DeathFadeDuration = 1.5f;
        const float RegenerationRate = 20f;
        const float RegenerationDelay = 2f;
        const float MaxVignetteOpacity = 0.8f;

        float _deathFadeAlpha;
        float _timeSinceLastDamage;
        Texture2D _vignetteTexture;

        Vector3 _initialCameraLocalPos;

        void Awake()
        {
            CreateVignetteTexture();
            _initialCameraLocalPos = _cameraTransform.localPosition;
            OnEnable();
        }

        void OnEnable() => ResetPlayerState();

        void ResetPlayerState()
        {
            CurrentHealth = MaxHealth;
            _timeSinceLastDamage = RegenerationDelay;
            _deathFadeAlpha = 0f;

            _playerMovement.enabled = true;
            _cameraController.enabled = true;
            _cameraController.LookDownRotation = 0;
            _cameraController.LookRightRotation = _cameraController.transform.parent.localEulerAngles.y;
            _smoothCamera.enabled = true;
            _cameraTransform.localPosition = _initialCameraLocalPos;
            _cameraTransform.localRotation = Quaternion.identity;
        }

        void Update()
        {
            if (IsDead) return;
            HandleHealthRegeneration();
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0 || IsDead) return;
            CurrentHealth -= amount;
            CurrentHealth = Mathf.Max(CurrentHealth, 0);
            _timeSinceLastDamage = 0f;
            if (IsDead) Die();
        }

        public void Heal(float amount)
        {
            if (amount <= 0 || IsDead) return;
            CurrentHealth += amount;
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        }

        void Die()
        {
            if (_deathFadeAlpha > 0f) return;

            _playerMovement.enabled = false;
            _cameraController.enabled = false;
            _smoothCamera.enabled = false;
            _playerMovement.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

            StartCoroutine(FadeToBlackThenRespawn());
        }

        IEnumerator FadeToBlackThenRespawn()
        {
            float timer = 0f;

            var startPos = _cameraTransform.localPosition;
            var startRot = _cameraTransform.localRotation;
            float rollDirection = Random.value < 0.5f ? 1f : -1f;
            var endPos = startPos + new Vector3(-rollDirection * 1.5f, -1f, 0);
            var endRot = startRot * Quaternion.Euler(0, 0, 90f * rollDirection);

            while (timer < DeathFadeDuration) {
                float progress = 1 - Mathf.Pow(1 - timer / DeathFadeDuration, 3);

                _deathFadeAlpha = Mathf.Lerp(0f, 1f, progress);
                _cameraTransform.localPosition = Vector3.Lerp(startPos, endPos, progress);
                _cameraTransform.localRotation = Quaternion.Slerp(startRot, endRot, progress);

                timer += Time.deltaTime;
                yield return null;
            }

            ResetPlayerState();
            SaveAndLoad.Instance.Load();
        }

        void OnGUI()
        {
            if (_deathFadeAlpha > 0f) {
                GUI.color = new Color(0, 0, 0, _deathFadeAlpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                return;
            }

            float damageIntensity = 1f - CurrentHealth / MaxHealth;
            if (damageIntensity > 0f && _vignetteTexture) {
                GUI.color = new Color(0.5f, 0f, 0f) {
                    a = damageIntensity * MaxVignetteOpacity
                };
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _vignetteTexture);
            }
        }

        void HandleHealthRegeneration()
        {
            if (_timeSinceLastDamage < RegenerationDelay) {
                _timeSinceLastDamage += Time.deltaTime;
            } else if (CurrentHealth < MaxHealth) {
                Heal(RegenerationRate * Time.deltaTime);
            }
        }

        void CreateVignetteTexture()
        {
            const int size = 128;
            _vignetteTexture = new Texture2D(size, size, TextureFormat.RGBA32, false) {
                name = "DamageVignette"
            };

            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    float normalizedDist = Vector2.Distance(new Vector2(size / 2f, size / 2f), new Vector2(x, y)) / (size / 2f);

                    _vignetteTexture.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(Mathf.Pow(normalizedDist, 2.5f))));
                }
            }
            _vignetteTexture.Apply();
        }
    }
}