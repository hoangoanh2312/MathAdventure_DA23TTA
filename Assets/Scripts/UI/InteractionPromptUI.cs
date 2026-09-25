using UnityEngine;
using UnityEngine.UI;

namespace MathAdventure.UI
{
    public sealed class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private Text label;
        private void Awake() { if (label == null) label = GetComponentInChildren<Text>(true); SetPrompt(false, string.Empty); }
        public void SetPrompt(bool visible, string text)
        {
            gameObject.SetActive(visible);
            if (label != null) label.text = string.IsNullOrWhiteSpace(text) ? "[E] TƯƠNG TÁC" : $"[E] {text}";
        }
    }
}
