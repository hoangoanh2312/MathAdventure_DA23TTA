using MathAdventure.Core;
using UnityEngine;

namespace MathAdventure.Collectibles
{
    public sealed class CoinCollectible : Collectible
    {
        [SerializeField, Min(0)] private int score = 10;
        [SerializeField, Min(0)] private int gold = 1;
        protected override bool Grant(GameObject collector)
        {
            if (GameManager.Instance == null) return false;
            GameManager.Instance.AddCoin(score, gold);
            GameEvents.RaiseCoinCollected();
            return true;
        }
    }
}
