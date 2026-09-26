using MathAdventure.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MathAdventure.UI
{
    public sealed class DialogueUI : MonoBehaviour
    {
        [SerializeField] private Text messageText;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (closeButton != null) { closeButton.onClick.RemoveListener(Hide); closeButton.onClick.AddListener(Hide); }
            gameObject.SetActive(false);
        }

        public void Show(string message)
        {
            if (messageText != null) messageText.text = message;
            gameObject.SetActive(true);
            if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.Paused);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.Playing);
        }
    }
}
