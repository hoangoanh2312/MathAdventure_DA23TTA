using UnityEngine;
using UnityEngine.UI;
using MathAdventure.World;

namespace MathAdventure.UI
{
    public sealed class IslandIntroUI : MonoBehaviour
    {
        [SerializeField] private Button mapButton;
        [SerializeField] private Button startButton;
        [SerializeField] private GameObject mapPanel;
        private Island01Manager island;

        private void Awake()
        {
            if (mapButton != null) mapButton.onClick.AddListener(ToggleMap);
            if (startButton != null) startButton.onClick.AddListener(Begin);
            if (mapPanel != null) mapPanel.SetActive(false);
        }

        public void Show(Island01Manager manager) { island = manager; gameObject.SetActive(true); }
        public void Hide() => gameObject.SetActive(false);
        private void ToggleMap() { if (mapPanel != null) mapPanel.SetActive(!mapPanel.activeSelf); }
        private void Begin() => island?.BeginIsland();
    }
}
