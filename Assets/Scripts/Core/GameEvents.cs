using System;

namespace MathAdventure.Core
{
    public static class GameEvents
    {
        public static event Action<int, int, int, int, int> StatsChanged;
        public static event Action<GameState> StateChanged;

        public static void RaiseStatsChanged(GameStats stats)
        {
            StatsChanged?.Invoke(stats.Score, stats.Gold, stats.Keys, stats.CorrectAnswers, stats.WrongAnswers);
        }

        public static void RaiseStateChanged(GameState state) => StateChanged?.Invoke(state);
    }
}
