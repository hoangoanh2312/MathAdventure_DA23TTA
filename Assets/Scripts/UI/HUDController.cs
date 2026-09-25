using MathAdventure.Core;
using MathAdventure.Player;
using UnityEngine;
using UnityEngine.UI;

namespace MathAdventure.UI
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private Text heartsText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text goldText;
        [SerializeField] private Text keyText;
        [SerializeField, Min(1)] private int keysNeeded = 1;
        private PlayerHealth health;

        private void OnEnable() { GameEvents.StatsChanged += UpdateStats; }
        private void Start()
        {
            health = FindFirstObjectByType<PlayerHealth>();
            if (health != null) { health.OnHealthChanged += UpdateHealth; UpdateHealth(health.CurrentHealth, 3); }
            if (GameManager.Instance != null) GameManager.Instance.NotifyStatsChanged();
        }
        private void OnDisable()
        {
            GameEvents.StatsChanged -= UpdateStats;
            if (health != null) health.OnHealthChanged -= UpdateHealth;
        }
        private void UpdateHealth(int current, int maximum) { if (heartsText != null) heartsText.text = $"Heart: {current}"; }
        private void UpdateStats(int score, int gold, int keys, int correct, int wrong)
        {
            if (scoreText != null) scoreText.text = $"Score: {score}";
            if (goldText != null) goldText.text = $"Gold: {gold}";
            if (keyText != null) keyText.text = $"Key: {keys}/{keysNeeded}";
        }
    }
}
