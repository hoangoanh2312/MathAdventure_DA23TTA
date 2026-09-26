using UnityEngine;
using UnityEngine.InputSystem;
using MathAdventure.Core;

namespace MathAdventure.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        private Rigidbody2D body;
        private InputAction moveAction;
        public Vector2 MoveInput { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            moveAction.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        }

        private void OnEnable() => moveAction.Enable();
        private void OnDisable() => moveAction.Disable();
        private void Update()
        {
            MoveInput = GameManager.Instance == null || GameManager.Instance.State == GameState.Playing
                ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f)
                : Vector2.zero;
        }
        private void FixedUpdate() => body.linearVelocity = MoveInput * moveSpeed;
    }
}
