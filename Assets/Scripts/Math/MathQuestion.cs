using System;

namespace MathAdventure.MathSystem
{
    public enum MathOperation
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Mixed
    }

    [Serializable]
    public sealed class MathQuestion
    {
        public string Expression { get; }
        public int CorrectAnswer { get; }
        public int[] Answers { get; }

        public MathQuestion(string expression, int correctAnswer, int[] answers)
        {
            Expression = expression;
            CorrectAnswer = correctAnswer;
            Answers = answers;
        }
    }
}
