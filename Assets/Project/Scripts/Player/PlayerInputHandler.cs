using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectFantasy.Player
{
    // 입력 수집과 선입력 버퍼링 전담
    [DisallowMultipleComponent]
    public sealed class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField, Min(0f)] private float inputBufferTime = 0.2f;

        private float lastAttackPressedTime = float.NegativeInfinity;
        private float lastJumpPressedTime = float.NegativeInfinity;

        public Vector2 MoveInput => moveAction.action.ReadValue<Vector2>();
        public bool IsSprintHeld => sprintAction.action.IsPressed();

        private void OnEnable()
        {
            attackAction.action.performed += OnAttackPerformed;
            jumpAction.action.performed += OnJumpPerformed;

            moveAction.action.Enable();
            attackAction.action.Enable();
            jumpAction.action.Enable();
            sprintAction.action.Enable();
        }

        private void OnDisable()
        {
            attackAction.action.performed -= OnAttackPerformed;
            jumpAction.action.performed -= OnJumpPerformed;
            ClearBuffers();
        }

        public bool ConsumeAttack() => ConsumeBuffered(ref lastAttackPressedTime);
        public bool ConsumeJump() => ConsumeBuffered(ref lastJumpPressedTime);

        public void ClearBuffers()
        {
            lastAttackPressedTime = float.NegativeInfinity;
            lastJumpPressedTime = float.NegativeInfinity;
        }

        private bool ConsumeBuffered(ref float pressedTime)
        {
            if (Time.time - pressedTime > inputBufferTime) return false;

            pressedTime = float.NegativeInfinity;
            return true;
        }

        private void OnAttackPerformed(InputAction.CallbackContext _) => lastAttackPressedTime = Time.time;
        private void OnJumpPerformed(InputAction.CallbackContext _) => lastJumpPressedTime = Time.time;
    }
}
