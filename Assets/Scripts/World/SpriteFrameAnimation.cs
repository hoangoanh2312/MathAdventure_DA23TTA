using UnityEngine;

namespace MathAdventure.World
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteFrameAnimation : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(1f)] private float framesPerSecond = 8f;
        private SpriteRenderer spriteRenderer;

        private void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;
            var index = Mathf.FloorToInt(Time.time * framesPerSecond) % frames.Length;
            spriteRenderer.sprite = frames[index];
        }
    }
}
