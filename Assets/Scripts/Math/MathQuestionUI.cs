using System;
using UnityEngine;
using UnityEngine.UI;

namespace MathAdventure.MathSystem
{
    public sealed class MathQuestionUI : MonoBehaviour
    {
        [SerializeField] private Text questionText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private Button continueButton;
        private Action<int> onAnswer;
        private Action onContinue;

        private void Awake()
        {
            if (continueButton != null) continueButton.onClick.AddListener(() => onContinue?.Invoke());
            gameObject.SetActive(false);
        }

        public void Show(MathQuestion question, Action<int> answerHandler, Action continueHandler)
        {
            onAnswer = answerHandler;
            onContinue = continueHandler;
            if (questionText != null) questionText.text = question.Expression;
            if (feedbackText != null) feedbackText.text = string.Empty;
            if (continueButton != null) continueButton.gameObject.SetActive(false);
            for (var i = 0; i < answerButtons.Length && i < question.Answers.Length; i++)
            {
                var value = question.Answers[i];
                var button = answerButtons[i];
                button.interactable = true;
                var label = button.GetComponentInChildren<Text>();
                if (label != null) label.text = value.ToString();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onAnswer?.Invoke(value));
            }
            gameObject.SetActive(true);
        }

        public void ShowFeedback(bool correct)
        {
            if (feedbackText != null) feedbackText.text = correct ? "CHÍNH XÁC!" : "CHƯA ĐÚNG!";
            if (!correct) return;
            foreach (var button in answerButtons) if (button != null) button.interactable = false;
            if (continueButton != null) continueButton.gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
