using MathAdventure.Interaction;
using MathAdventure.MathSystem;
using UnityEngine;

namespace MathAdventure.World
{
    public sealed class MathShrine : MonoBehaviour, IInteractable
    {
        [SerializeField] private MathQuestionManager mathManager;
        public string PromptText => "GIẢI TOÁN";
        public bool CanInteract => mathManager != null && !mathManager.IsSolved;
        public void Interact() => mathManager.Open();
    }
}
