using UnityEngine;
using UnityEngine.Events;

namespace GravityGame.Player
{
    /// <summary>
    /// Manages the player's health, including taking damage, healing, and death.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerHealth : MonoBehaviour
    {
        public static PlayerHealth Instance { get; private set; }
        
        public UnityEvent OnPlayerDied;
        public static float MaxHealth => 100f;
        public float CurrentHealth { get; private set; }
        bool IsDead => CurrentHealth <= 0;

        void OnEnable()
        {
            if (!Instance) Instance = this;
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

        void Die() => OnPlayerDied?.Invoke();
    }
}