using System.Collections;
using MathAdventure.Core;
using MathAdventure.Objectives;
using MathAdventure.Player;
using MathAdventure.UI;
using UnityEngine;

namespace MathAdventure.World
{
    public sealed class Island01Manager : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Transform player;
        [SerializeField] private Transform startCheckpoint;
        [SerializeField] private ObjectiveManager objectives;
        [SerializeField] private IslandIntroUI introUI;
        [SerializeField] private CompletionUI completionUI;
        [SerializeField] private GameObject deathPanel;
        private bool respawning;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetProgress();
                GameManager.Instance.SetState(GameState.Paused);
            }
            if (playerHealth != null) playerHealth.OnPlayerDeath += OnPlayerDeath;
            if (deathPanel != null) deathPanel.SetActive(false);
            if (introUI != null) introUI.Show(this);
        }

        private void OnDestroy()
        {
            if (playerHealth != null) playerHealth.OnPlayerDeath -= OnPlayerDeath;
        }

        public void BeginIsland()
        {
            if (introUI != null) introUI.Hide();
            if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.Playing);
        }

        public void CompleteIsland()
        {
            if (objectives == null || objectives.LevelCompleted) return;
            objectives.CompleteLevel();
            GameEvents.RaiseIslandCompleted();
            if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.Paused);
            if (completionUI != null) completionUI.Show();
        }

        public void ContinueAfterCompletion()
        {
            if (completionUI != null) completionUI.Hide();
            Debug.Log("Island 1 completion hook invoked. Phase 3 will connect the World Map.");
        }

        private void OnPlayerDeath()
        {
            if (!respawning) StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            respawning = true;
            if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.GameOver);
            if (deathPanel != null) deathPanel.SetActive(true);
            yield return new WaitForSecondsRealtime(1.25f);
            if (player != null && startCheckpoint != null) player.position = startCheckpoint.position;
            if (playerHealth != null) playerHealth.RestoreFull();
            if (deathPanel != null) deathPanel.SetActive(false);
            if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.Playing);
            respawning = false;
        }
    }
}
