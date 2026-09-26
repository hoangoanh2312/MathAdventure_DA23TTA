using System;

namespace MathAdventure.Core
{
    public static class GameEvents
    {
        public static event Action<int, int, int, int, int> StatsChanged;
        public static event Action<GameState> StateChanged;
        public static event Action CoinCollected;
        public static event Action KeyCollected;
        public static event Action IslandCompleted;

        public static void RaiseStatsChanged(GameStats stats)
        {
            StatsChanged?.Invoke(stats.Score, stats.Gold, stats.Keys, stats.CorrectAnswers, stats.WrongAnswers);
        }

        public static void RaiseStateChanged(GameState state) => StateChanged?.Invoke(state);
        public static void RaiseCoinCollected() => CoinCollected?.Invoke();
        public static void RaiseKeyCollected() => KeyCollected?.Invoke();
        public static void RaiseIslandCompleted() => IslandCompleted?.Invoke();
    }
}
