using System.Collections;
using UnityEngine;

namespace MathAdventure.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class PanelFade : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float duration = 0.25f;
        private void OnEnable() { StopAllCoroutines(); StartCoroutine(FadeIn()); }
        private IEnumerator FadeIn()
        {
            var group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
            for (var elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            group.alpha = 1f;
        }
    }
}
