using System.Collections;
using UnityEngine;

namespace MathAdventure.Player
{
    [RequireComponent(typeof(PlayerHealth))]
    public sealed class DamageFlash : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] renderers;
        [SerializeField, Min(0.02f)] private float interval = 0.08f;
        private PlayerHealth health;
        private int previousHealth;
        private Coroutine routine;

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
            if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<SpriteRenderer>(true);
            previousHealth = health.CurrentHealth;
        }

        private void OnEnable() => health.OnHealthChanged += OnHealthChanged;
        private void OnDisable() => health.OnHealthChanged -= OnHealthChanged;

        private void OnHealthChanged(int current, int maximum)
        {
            if (current < previousHealth)
            {
                if (routine != null) StopCoroutine(routine);
                routine = StartCoroutine(FlashRoutine());
            }
            previousHealth = current;
        }

        private IEnumerator FlashRoutine()
        {
            for (var i = 0; i < 6; i++)
            {
                foreach (var item in renderers) if (item != null) item.enabled = i % 2 != 0;
                yield return new WaitForSeconds(interval);
            }
            foreach (var item in renderers) if (item != null) item.enabled = true;
            routine = null;
        }
    }
}
