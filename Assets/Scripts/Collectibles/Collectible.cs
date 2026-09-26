using UnityEngine;

namespace MathAdventure.Collectibles
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class Collectible : MonoBehaviour
    {
        private bool collected;
        protected abstract bool Grant(GameObject collector);
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || !other.CompareTag("Player") || !Grant(other.gameObject)) return;
            collected = true;
            var collider = GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;
            if (TryGetComponent<CollectibleVisual>(out var visual)) visual.PlayCollected(() => gameObject.SetActive(false));
            else gameObject.SetActive(false);
        }
        protected virtual void Reset() => GetComponent<Collider2D>().isTrigger = true;
    }
}
