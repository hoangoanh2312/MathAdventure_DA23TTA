using UnityEngine;

namespace MathAdventure.World
{
    public sealed class SimpleYSort : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] renderers;
        [SerializeField] private int baseOrder = 100;
        private int[] offsets;

        private void Awake()
        {
            if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<SpriteRenderer>(true);
            offsets = new int[renderers.Length];
            for (var i = 0; i < renderers.Length; i++) offsets[i] = renderers[i].sortingOrder;
        }

        private void LateUpdate()
        {
            var order = baseOrder - Mathf.RoundToInt(transform.position.y * 10f);
            for (var i = 0; i < renderers.Length; i++) if (renderers[i] != null) renderers[i].sortingOrder = order + offsets[i];
        }
    }
}
