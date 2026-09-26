using System.Collections;
using UnityEngine;

namespace MathAdventure.World
{
    public sealed class ChestVisual : MonoBehaviour
    {
        [SerializeField] private Transform lid;
        [SerializeField] private SpriteRenderer glow;
        private bool opened;

        public void Open()
        {
            if (!opened) StartCoroutine(OpenRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            opened = true;
            var start = lid != null ? lid.localPosition : Vector3.zero;
            for (var elapsed = 0f; elapsed < 0.3f; elapsed += Time.deltaTime)
            {
                var progress = Mathf.Clamp01(elapsed / 0.3f);
                if (lid != null) lid.localPosition = start + new Vector3(0f, Mathf.Lerp(0f, 0.45f, progress), 0f);
                if (glow != null) glow.color = new Color(1f, 0.9f, 0.25f, progress * 0.8f);
                yield return null;
            }
        }
    }
}
