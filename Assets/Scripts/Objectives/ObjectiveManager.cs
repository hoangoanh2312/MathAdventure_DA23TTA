using System;
using MathAdventure.Core;
using UnityEngine;

namespace MathAdventure.Objectives
{
    public enum IslandObjective
    {
        MeetNpc,
        CollectCoins,
        FindKey,
        ReachMathShrine,
        SolveMath,
        OpenFinalChest,
        Completed
    }

    public sealed class ObjectiveManager : MonoBehaviour
    {
        [SerializeField, Min(1)] private int requiredCoins = 10;
        public event Action<string> ObjectiveChanged;
        public IslandObjective Current { get; private set; } = IslandObjective.MeetNpc;
        public int CoinsCollected { get; private set; }
        public bool HasKey { get; private set; }
        public bool MathSolved { get; private set; }
        public bool LevelCompleted { get; private set; }

        private void OnEnable()
        {
            GameEvents.CoinCollected += OnCoinCollected;
            GameEvents.KeyCollected += OnKeyCollected;
        }

        private void Start() => Publish();

        private void OnDisable()
        {
            GameEvents.CoinCollected -= OnCoinCollected;
            GameEvents.KeyCollected -= OnKeyCollected;
        }

        public void MeetNpc()
        {
            if (Current != IslandObjective.MeetNpc) return;
            Current = CoinsCollected >= requiredCoins ? IslandObjective.FindKey : IslandObjective.CollectCoins;
            Publish();
        }

        public void ReachShrine()
        {
            if (Current == IslandObjective.ReachMathShrine)
            {
                Current = IslandObjective.SolveMath;
                Publish();
            }
        }

        public void SolveMath()
        {
            if (MathSolved) return;
            MathSolved = true;
            Current = IslandObjective.OpenFinalChest;
            Publish();
        }

        public void CompleteLevel()
        {
            if (LevelCompleted) return;
            LevelCompleted = true;
            Current = IslandObjective.Completed;
            Publish();
        }

        public string GetNpcDialogue()
        {
            if (LevelCompleted) return "Bạn đã chinh phục hòn đảo này. Làm tốt lắm!";
            if (MathSolved) return "Tuyệt vời! Hãy đến khu kho báu cuối đảo.";
            if (HasKey) return "Bạn đã có chìa khóa. Hãy tới Đền Toán và vượt thử thách.";
            if (CoinsCollected >= requiredCoins) return "Tốt lắm! Hãy tìm chiếc chìa khóa ở khu vực phía bên kia.";
            return "Chào mừng đến với hòn đảo đầu tiên!\nHãy khám phá và thu thập những đồng xu trên đường.";
        }

        private void OnCoinCollected()
        {
            CoinsCollected = Mathf.Min(requiredCoins, CoinsCollected + 1);
            if (Current == IslandObjective.CollectCoins && CoinsCollected >= requiredCoins)
                Current = HasKey ? IslandObjective.ReachMathShrine : IslandObjective.FindKey;
            Publish();
        }

        private void OnKeyCollected()
        {
            HasKey = true;
            if (Current == IslandObjective.FindKey || (Current == IslandObjective.CollectCoins && CoinsCollected >= requiredCoins))
                Current = IslandObjective.ReachMathShrine;
            Publish();
        }

        private void Publish() => ObjectiveChanged?.Invoke(GetObjectiveText());

        private string GetObjectiveText()
        {
            return Current switch
            {
                IslandObjective.MeetNpc => "Gặp người dân trên đảo",
                IslandObjective.CollectCoins => $"Thu thập xu: {CoinsCollected}/{requiredCoins}",
                IslandObjective.FindKey => "Tìm chìa khóa",
                IslandObjective.ReachMathShrine => "Đến Đền Toán",
                IslandObjective.SolveMath => "Giải thử thách toán",
                IslandObjective.OpenFinalChest => "Mở kho báu cuối đảo",
                IslandObjective.Completed => "Đảo 1 đã hoàn thành",
                _ => string.Empty
            };
        }
    }
}
