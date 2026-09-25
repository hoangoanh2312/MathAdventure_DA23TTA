using System;
using UnityEngine;

namespace MathAdventure.Core
{
    [Serializable]
    public sealed class GameStats
    {
        [field: SerializeField] public int Score { get; private set; }
        [field: SerializeField] public int Gold { get; private set; }
        [field: SerializeField] public int Keys { get; private set; }
        [field: SerializeField] public int CorrectAnswers { get; private set; }
        [field: SerializeField] public int WrongAnswers { get; private set; }

        public void AddScore(int amount) => Score = Mathf.Max(0, Score + amount);
        public void AddGold(int amount) => Gold = Mathf.Max(0, Gold + amount);
        public bool TrySpendKey()
        {
            if (Keys <= 0) return false;
            Keys--;
            return true;
        }
        public void AddKey(int amount = 1) => Keys = Mathf.Max(0, Keys + amount);
        public void RecordAnswer(bool correct)
        {
            if (correct) CorrectAnswers++;
            else WrongAnswers++;
        }
    }
}
