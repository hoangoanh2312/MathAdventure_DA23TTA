using System;
using System.Collections;
using UnityEngine;

namespace MathAdventure.Collectibles
{
    public sealed class CollectibleVisual : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField, Min(0f)] private float bobHeight = 0.12f;
        [SerializeField, Min(0f)] private float bobSpeed = 2.5f;
        [SerializeField] private bool pulse;
        private Vector3 startPosition;
        private Vector3 startScale;
        private bool collecting;

        private void Awake()
        {
            if (visualRoot == null) visualRoot = transform;
            startPosition = visualRoot.localPosition;
            startScale = visualRoot.localScale;
        }

        private void Update()
        {
            if (collecting || visualRoot == null) return;
            var wave = Mathf.Sin(Time.time * bobSpeed);
            visualRoot.localPosition = startPosition + Vector3.up * (wave * bobHeight);
            if (pulse) visualRoot.localScale = startScale * (1f + wave * 0.06f);
        }

        public void PlayCollected(Action finished)
        {
            if (!collecting) StartCoroutine(CollectRoutine(finished));
        }

        private IEnumerator CollectRoutine(Action finished)
        {
            collecting = true;
            var elapsed = 0f;
            const float duration = 0.18f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);
                visualRoot.localScale = startScale * Mathf.Lerp(1f, 1.55f, progress);
                visualRoot.localPosition = startPosition + Vector3.up * Mathf.Lerp(0f, 0.4f, progress);
                yield return null;
            }
            finished?.Invoke();
        }
    }
}
