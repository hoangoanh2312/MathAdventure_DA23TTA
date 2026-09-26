using MathAdventure.Objectives;
using UnityEngine;
using UnityEngine.UI;

namespace MathAdventure.UI
{
    public sealed class ObjectiveUI : MonoBehaviour
    {
        [SerializeField] private ObjectiveManager objectives;
        [SerializeField] private Text objectiveText;
        private void OnEnable() { if (objectives != null) objectives.ObjectiveChanged += UpdateObjective; }
        private void Start() { if (objectiveText != null) objectiveText.text = "Gặp người dân trên đảo"; }
        private void OnDisable() { if (objectives != null) objectives.ObjectiveChanged -= UpdateObjective; }
        private void UpdateObjective(string text) { if (objectiveText != null) objectiveText.text = "NHIỆM VỤ\n" + text; }
    }
}
