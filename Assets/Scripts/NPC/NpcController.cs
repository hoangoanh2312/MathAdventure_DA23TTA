using MathAdventure.Interaction;
using MathAdventure.Objectives;
using MathAdventure.UI;
using UnityEngine;

namespace MathAdventure.NPC
{
    public sealed class NpcController : MonoBehaviour, IInteractable
    {
        [SerializeField] private ObjectiveManager objectives;
        [SerializeField] private DialogueUI dialogueUI;
        public string PromptText => "NÓI CHUYỆN";
        public bool CanInteract => dialogueUI != null && objectives != null;

        public void Interact()
        {
            dialogueUI.Show(objectives.GetNpcDialogue());
            objectives.MeetNpc();
        }
    }
}
