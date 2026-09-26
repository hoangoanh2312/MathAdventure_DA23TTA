using MathAdventure.Interaction;
using MathAdventure.Objectives;
using MathAdventure.UI;
using UnityEngine;

namespace MathAdventure.World
{
    public sealed class FinalChest : MonoBehaviour, IInteractable
    {
        [SerializeField] private ObjectiveManager objectives;
        [SerializeField] private Island01Manager island;
        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private ChestVisual chestVisual;
        private bool opened;
        public string PromptText => "MỞ KHO BÁU";
        public bool CanInteract => !opened;

        public void Interact()
        {
            if (opened || objectives == null) return;
            if (!objectives.HasKey) { dialogueUI?.Show("Bạn vẫn chưa tìm thấy chìa khóa."); return; }
            if (!objectives.MathSolved) { dialogueUI?.Show("Hãy vượt qua thử thách toán trước."); return; }
            opened = true;
            chestVisual?.Open();
            island?.CompleteIsland();
        }
    }
}
