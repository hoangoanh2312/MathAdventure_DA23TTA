using MathAdventure.Core;
using UnityEngine;

namespace MathAdventure.Player
{
    public sealed class PlayerInventory : MonoBehaviour
    {
        public int KeyCount => GameManager.Instance != null ? GameManager.Instance.Stats.Keys : 0;
        public bool UseKey() => GameManager.Instance != null && GameManager.Instance.TrySpendKey();
    }
}
