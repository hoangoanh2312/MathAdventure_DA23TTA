using MathAdventure.Core;
using MathAdventure.World;
using UnityEngine;
using UnityEngine.UI;

namespace MathAdventure.UI
{
    public sealed class CompletionUI : MonoBehaviour
    {
        [SerializeField] private Text summaryText;
        [SerializeField] private Button continueButton;
        [SerializeField] private Island01Manager island;

        private void Awake()
        {
            if (continueButton != null) continueButton.onClick.AddListener(Continue);
            gameObject.SetActive(false);
        }

        public void Show()
        {
            if (summaryText != null && GameManager.Instance != null)
            {
                var stats = GameManager.Instance.Stats;
                summaryText.text = $"HOÀN THÀNH ĐẢO 1!\n\nĐiểm: {stats.Score}\nVàng: {stats.Gold}\nCâu đúng: {stats.CorrectAnswers}\nCâu sai: {stats.WrongAnswers}";
            }
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
        private void Continue() => island?.ContinueAfterCompletion();
    }
}
