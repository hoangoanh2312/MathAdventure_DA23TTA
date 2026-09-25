using MathAdventure.Player;
using UnityEngine;

namespace MathAdventure.World
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class DamageZone : MonoBehaviour
    {
        [SerializeField, Min(1)] private int damage = 1;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerHealth>(out var health)) health.TakeDamage(damage);
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<PlayerHealth>(out var health)) health.TakeDamage(damage);
        }
    }
}
