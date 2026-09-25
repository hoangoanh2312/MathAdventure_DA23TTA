using UnityEngine;

namespace MathAdventure.Player
{
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private PlayerController controller;
        private Vector2 facing = Vector2.down;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            var movement = controller.MoveInput;
            if (movement.sqrMagnitude > 0.001f) facing = movement.normalized;
            if (animator == null || animator.runtimeAnimatorController == null) return;
            SetFloatIfPresent("MoveX", movement.x);
            SetFloatIfPresent("MoveY", movement.y);
            SetFloatIfPresent("FacingX", facing.x);
            SetFloatIfPresent("FacingY", facing.y);
            SetFloatIfPresent("Speed", movement.sqrMagnitude);
        }

        private void SetFloatIfPresent(string name, float value)
        {
            foreach (var parameter in animator.parameters)
                if (parameter.name == name && parameter.type == AnimatorControllerParameterType.Float) { animator.SetFloat(name, value); return; }
        }
    }
}
