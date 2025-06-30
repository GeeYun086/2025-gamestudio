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

        static readonly Color EffectColor = new(1f, 0f, 0f, 0.6f);
        const float IndicatorHeight = 100f;

        float _deathFadeAlpha;
        float _timeSinceLastDamage;
        Texture2D _vignetteTexture;
        Texture2D _healthIndicatorTexture;

        void Awake()
        {
            CreateVignetteTexture();
            CreateHealthIndicatorTexture();
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

            float damageAlpha = Mathf.Max(1f - CurrentHealth / MaxHealth, Mathf.Max(0f, 1f - _timeSinceLastDamage / RegenerationDelay));
            if (damageAlpha > 0f) {
                GUI.color = new Color(EffectColor.r, EffectColor.g, EffectColor.b, EffectColor.a * damageAlpha);
                DrawDamagedHealthIndicator();
                DrawVignette();

                GUI.color = Color.white;
            }
        }

        void DrawVignette()
        {
            if (_vignetteTexture) GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _vignetteTexture);
        }

        void DrawDamagedHealthIndicator()
        {
            if (!_healthIndicatorTexture) return;
            float indicatorWidth = Screen.width * Mathf.Clamp01(CurrentHealth / MaxHealth);
            GUI.DrawTexture(
                new Rect(0, Screen.height - IndicatorHeight, indicatorWidth, IndicatorHeight), _healthIndicatorTexture, ScaleMode.StretchToFill
            );
        }

        void HandleHealthRegeneration()
        {
            _timeSinceLastDamage += Time.deltaTime;
            if (_timeSinceLastDamage >= RegenerationDelay && CurrentHealth < MaxHealth) Heal(RegenerationRate * Time.deltaTime);
        }

        void CreateHealthIndicatorTexture()
        {
            const int indicatorHeight = 64;
            _healthIndicatorTexture = new Texture2D(1, indicatorHeight, TextureFormat.RGBA32, false) {
                name = "HealthIndicator",
                wrapMode = TextureWrapMode.Clamp
            };

            for (int y = 0; y < indicatorHeight; y++)
                _healthIndicatorTexture.SetPixel(0, y, new Color(1, 1, 1, Mathf.Pow(1.0f - y / (float)indicatorHeight, 1.5f)));
            _healthIndicatorTexture.Apply();
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