using MathAdventure.Core;
using MathAdventure.Objectives;
using UnityEngine;

namespace MathAdventure.MathSystem
{
    public sealed class MathQuestionManager : MonoBehaviour
    {
        [SerializeField] private MathQuestionUI questionUI;
        [SerializeField] private ObjectiveManager objectives;
        [SerializeField] private MathOperation operation = MathOperation.Mixed;
        [SerializeField, Min(1)] private int maximum = 20;
        private MathQuestion current;
        private bool rewarded;

        public bool IsSolved => objectives != null && objectives.MathSolved;

        public void Open()
        {
            if (IsSolved || questionUI == null) return;
            objectives?.ReachShrine();
            current = MathQuestionGenerator.Generate(operation, maximum);
            if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.Paused);
            questionUI.Show(current, SubmitAnswer, Continue);
        }

        private void SubmitAnswer(int answer)
        {
            if (current == null || rewarded) return;
            if (answer != current.CorrectAnswer)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddScore(-25);
                    GameManager.Instance.RecordAnswer(false);
                }
                questionUI.ShowFeedback(false);
                return;
            }

            rewarded = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(100);
                GameManager.Instance.RecordAnswer(true);
            }
            objectives?.SolveMath();
            questionUI.ShowFeedback(true);
        }

        private void Continue()
        {
            questionUI.Hide();
            if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.Playing);
        }
    }
}
