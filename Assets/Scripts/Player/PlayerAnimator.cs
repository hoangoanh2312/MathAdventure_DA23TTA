using UnityEngine;

namespace MathAdventure.Player
{
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Transform visualRoot;
        private PlayerController controller;
        private Vector2 facing = Vector2.down;
        private Vector3 visualStartPosition;
        private Vector3 visualStartScale;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (visualRoot == null)
            {
                var child = transform.Find("Visual");
                if (child != null) visualRoot = child;
            }
            if (visualRoot != null)
            {
                visualStartPosition = visualRoot.localPosition;
                visualStartScale = visualRoot.localScale;
            }
        }

        private void Update()
        {
            var movement = controller.MoveInput;
            if (movement.sqrMagnitude > 0.001f) facing = movement.normalized;
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                UpdateFallbackVisual(movement);
                return;
            }
            SetFloatIfPresent("MoveX", movement.x);
            SetFloatIfPresent("MoveY", movement.y);
            SetFloatIfPresent("FacingX", facing.x);
            SetFloatIfPresent("FacingY", facing.y);
            SetFloatIfPresent("Speed", movement.sqrMagnitude);
        }

        private void UpdateFallbackVisual(Vector2 movement)
        {
            if (visualRoot == null) return;
            var moving = movement.sqrMagnitude > 0.001f;
            var bob = moving ? Mathf.Abs(Mathf.Sin(Time.time * 10f)) * 0.08f : 0f;
            visualRoot.localPosition = visualStartPosition + Vector3.up * bob;
            var horizontalSign = Mathf.Abs(facing.x) > 0.25f && facing.x < 0f ? -1f : 1f;
            visualRoot.localScale = new Vector3(Mathf.Abs(visualStartScale.x) * horizontalSign, visualStartScale.y * (moving ? 0.96f + bob * 0.5f : 1f), visualStartScale.z);
        }

        private void SetFloatIfPresent(string name, float value)
        {
            foreach (var parameter in animator.parameters)
                if (parameter.name == name && parameter.type == AnimatorControllerParameterType.Float) { animator.SetFloat(name, value); return; }
        }
    }
}
