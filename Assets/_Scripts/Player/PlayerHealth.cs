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
        float _deathFadeAlpha;

        void Awake() => OnEnable();

        void OnEnable()
        {
            CurrentHealth = MaxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0) return;
            CurrentHealth -= amount;
            CurrentHealth = Mathf.Max(CurrentHealth, 0);
            if (IsDead) Die();
        }

        public void Heal(float amount)
        {
            if (amount <= 0) return;
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
            _deathFadeAlpha = 0f;
        }

        void OnGUI()
        {
            if (_deathFadeAlpha > 0f) {
                GUI.color = new Color(0, 0, 0, _deathFadeAlpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            }
        }
    }
}