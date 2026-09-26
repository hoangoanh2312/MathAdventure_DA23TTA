using MathAdventure.Core;
using UnityEngine;

namespace MathAdventure.Collectibles
{
    public sealed class KeyCollectible : Collectible
    {
        protected override bool Grant(GameObject collector)
        {
            if (GameManager.Instance == null) return false;
            GameManager.Instance.AddKey();
            GameEvents.RaiseKeyCollected();
            return true;
        }
    }
}
