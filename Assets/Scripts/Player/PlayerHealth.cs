using System;
using UnityEngine;
using MathAdventure.Core;

namespace MathAdventure.Player
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxHealth = 3;
        [SerializeField, Min(0f)] private float invulnerabilitySeconds = 1f;
        public event Action<int, int> OnHealthChanged;
        public event Action OnPlayerDeath;
        public int CurrentHealth { get; private set; }
        private float invulnerableUntil;

        private void Awake() => CurrentHealth = maxHealth;
        private void Start() => OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        public bool TakeDamage(int amount)
        {
            if (amount <= 0 || CurrentHealth <= 0 || Time.time < invulnerableUntil) return false;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            invulnerableUntil = Time.time + invulnerabilitySeconds;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            if (CurrentHealth == 0)
            {
                OnPlayerDeath?.Invoke();
                if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.GameOver);
            }
            return true;
        }

        public void RestoreFull()
        {
            CurrentHealth = maxHealth;
            invulnerableUntil = Time.time + invulnerabilitySeconds;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}
