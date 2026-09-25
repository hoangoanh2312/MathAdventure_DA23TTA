using UnityEngine;

namespace MathAdventure.World
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0.01f)] private float smoothTime = 0.2f;
        [SerializeField] private Vector2 offset;
        private Vector3 velocity;
        public void SetTarget(Transform value) => target = value;
        private void LateUpdate()
        {
            if (target == null) return;
            var desired = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }
    }
}
