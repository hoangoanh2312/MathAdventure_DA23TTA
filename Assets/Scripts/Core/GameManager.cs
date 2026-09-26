using UnityEngine;

namespace MathAdventure.Core
{
    [DefaultExecutionOrder(-100)]
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        [field: SerializeField] public GameState State { get; private set; } = GameState.Playing;
        [field: SerializeField] public GameStats Stats { get; private set; } = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetState(GameState state)
        {
            if (State == state) return;
            State = state;
            GameEvents.RaiseStateChanged(State);
        }

        public void AddCoin(int score = 10, int gold = 1)
        {
            Stats.AddScore(score);
            Stats.AddGold(gold);
            NotifyStatsChanged();
        }

        public void AddKey(int amount = 1) { Stats.AddKey(amount); NotifyStatsChanged(); }
        public void AddScore(int amount) { Stats.AddScore(amount); NotifyStatsChanged(); }
        public bool TrySpendKey() { var spent = Stats.TrySpendKey(); if (spent) NotifyStatsChanged(); return spent; }
        public void RecordAnswer(bool correct) { Stats.RecordAnswer(correct); NotifyStatsChanged(); }
        public void ResetProgress() { Stats.Reset(); SetState(GameState.Playing); NotifyStatsChanged(); }
        public void NotifyStatsChanged() => GameEvents.RaiseStatsChanged(Stats);
    }
}
