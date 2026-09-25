using System.Collections.Generic;
using MathAdventure.Interaction;
using MathAdventure.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MathAdventure.Player
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private InteractionPromptUI promptUI;
        private readonly List<IInteractable> nearby = new();
        private InputAction interactAction;

        private void Awake()
        {
            interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
            if (promptUI == null) promptUI = FindFirstObjectByType<InteractionPromptUI>();
        }
        private void OnEnable() { interactAction.performed += OnInteract; interactAction.Enable(); }
        private void OnDisable() { interactAction.performed -= OnInteract; interactAction.Disable(); }
        private void Update()
        {
            nearby.RemoveAll(item => item == null || !item.CanInteract);
            var target = nearby.Count > 0 ? nearby[^1] : null;
            if (promptUI != null) promptUI.SetPrompt(target != null, target?.PromptText ?? string.Empty);
        }
        private void OnInteract(InputAction.CallbackContext _) { if (nearby.Count > 0 && nearby[^1].CanInteract) nearby[^1].Interact(); }
        private void OnTriggerEnter2D(Collider2D other)
        {
            foreach (var behaviour in other.GetComponents<MonoBehaviour>()) if (behaviour is IInteractable item && !nearby.Contains(item)) nearby.Add(item);
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            foreach (var behaviour in other.GetComponents<MonoBehaviour>()) if (behaviour is IInteractable item) nearby.Remove(item);
        }
    }
}
