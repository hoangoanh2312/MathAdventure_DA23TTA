using System.Collections.Generic;
using UnityEngine;

namespace MathAdventure.MathSystem
{
    public static class MathQuestionGenerator
    {
        public static MathQuestion Generate(MathOperation operation, int maximum = 20)
        {
            var selected = operation == MathOperation.Mixed
                ? (Random.value < 0.5f ? MathOperation.Addition : MathOperation.Subtraction)
                : operation;
            int left;
            int right;
            int answer;
            string symbol;

            switch (selected)
            {
                case MathOperation.Subtraction:
                    left = Random.Range(0, maximum + 1);
                    right = Random.Range(0, left + 1);
                    answer = left - right;
                    symbol = "−";
                    break;
                case MathOperation.Multiplication:
                    left = Random.Range(1, maximum + 1);
                    right = Random.Range(1, maximum + 1);
                    answer = left * right;
                    symbol = "×";
                    break;
                case MathOperation.Division:
                    right = Random.Range(1, maximum + 1);
                    answer = Random.Range(1, maximum + 1);
                    left = right * answer;
                    symbol = "÷";
                    break;
                default:
                    left = Random.Range(0, maximum + 1);
                    right = Random.Range(0, maximum - left + 1);
                    answer = left + right;
                    symbol = "+";
                    break;
            }

            var values = new HashSet<int> { answer };
            while (values.Count < 4) values.Add(Mathf.Max(0, answer + Random.Range(-6, 7)));
            var answers = new int[4];
            values.CopyTo(answers);
            for (var i = answers.Length - 1; i > 0; i--)
            {
                var swap = Random.Range(0, i + 1);
                (answers[i], answers[swap]) = (answers[swap], answers[i]);
            }
            return new MathQuestion($"{left} {symbol} {right} = ?", answer, answers);
        }
    }
}
