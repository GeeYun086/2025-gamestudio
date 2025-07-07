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
        public UnityEvent OnPlayerDied;
        public static float MaxHealth => 100f;
        public float CurrentHealth { get; private set; }
        bool IsDead => CurrentHealth <= 0;

        const float DeathFadeDuration = 1.0f;
        const float RegenerationRate = 20f;
        const float RegenerationDelay = 2f;
        const float MaxVignetteOpacity = 0.8f;

        float _deathFadeAlpha;
        float _timeSinceLastDamage;
        Texture2D _vignetteTexture;

        void Awake()
        {
            CreateVignetteTexture();
            OnEnable();
        }

        void OnEnable() => ResetPlayerState();

        void ResetPlayerState()
        {
            CurrentHealth = MaxHealth;
            _timeSinceLastDamage = RegenerationDelay;
            _deathFadeAlpha = 0f;
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
            StartCoroutine(FadeToBlackThenRespawn());
        }

        IEnumerator FadeToBlackThenRespawn()
        {
            float timer = 0f;
            while (timer < DeathFadeDuration) {
                _deathFadeAlpha = Mathf.Lerp(0f, 1f, timer / DeathFadeDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            OnPlayerDied?.Invoke();
            SaveAndLoad.Instance.Load();
            ResetPlayerState();
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