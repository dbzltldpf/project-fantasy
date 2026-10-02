using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace ProjectFantasy.Player
{
    // 입력 수집과 선입력 버퍼링 전담
    [DisallowMultipleComponent]
    public sealed class PlayerInputHandler : MonoBehaviour
    {
        public const int NoWeaponCycle = 0;
        public const int NextWeaponCycle = 1;
        public const int PreviousWeaponCycle = -1;

        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;
        [Tooltip("무기에 따라 가드 또는 조준")]
        [SerializeField, FormerlySerializedAs("guardAction")] private InputActionReference secondaryAction;
        [SerializeField] private InputActionReference nextWeaponAction;
        [SerializeField] private InputActionReference previousWeaponAction;
        [SerializeField, Min(0f)] private float inputBufferTime = 0.2f;

        private float lastAttackPressedTime = float.NegativeInfinity;
        private float lastJumpPressedTime = float.NegativeInfinity;
        private float lastNextWeaponPressedTime = float.NegativeInfinity;
        private float lastPreviousWeaponPressedTime = float.NegativeInfinity;

        public Vector2 MoveInput => moveAction.action.ReadValue<Vector2>();
        public bool IsSprintHeld => sprintAction.action.IsPressed();
        public bool IsSecondaryHeld => secondaryAction.action.IsPressed();

        private void OnEnable()
        {
            attackAction.action.performed += OnAttackPerformed;
            jumpAction.action.performed += OnJumpPerformed;
            nextWeaponAction.action.performed += OnNextWeaponPerformed;
            previousWeaponAction.action.performed += OnPreviousWeaponPerformed;

            moveAction.action.Enable();
            attackAction.action.Enable();
            jumpAction.action.Enable();
            sprintAction.action.Enable();
            secondaryAction.action.Enable();
            nextWeaponAction.action.Enable();
            previousWeaponAction.action.Enable();
        }

        private void OnDisable()
        {
            attackAction.action.performed -= OnAttackPerformed;
            jumpAction.action.performed -= OnJumpPerformed;
            nextWeaponAction.action.performed -= OnNextWeaponPerformed;
            previousWeaponAction.action.performed -= OnPreviousWeaponPerformed;
            ClearBuffers();
        }

        public bool ConsumeAttack() => ConsumeBuffered(ref lastAttackPressedTime);
        public bool ConsumeJump() => ConsumeBuffered(ref lastJumpPressedTime);

        // 버퍼된 무기 교체 방향 반환 (없으면 NoWeaponCycle)
        public int ConsumeWeaponCycle()
        {
            if (ConsumeBuffered(ref lastNextWeaponPressedTime)) return NextWeaponCycle;
            if (ConsumeBuffered(ref lastPreviousWeaponPressedTime)) return PreviousWeaponCycle;
            return NoWeaponCycle;
        }

        public void ClearBuffers()
        {
            lastAttackPressedTime = float.NegativeInfinity;
            lastJumpPressedTime = float.NegativeInfinity;
            lastNextWeaponPressedTime = float.NegativeInfinity;
            lastPreviousWeaponPressedTime = float.NegativeInfinity;
        }

        private bool ConsumeBuffered(ref float pressedTime)
        {
            if (Time.time - pressedTime > inputBufferTime) return false;

            pressedTime = float.NegativeInfinity;
            return true;
        }

        private void OnAttackPerformed(InputAction.CallbackContext _) => lastAttackPressedTime = Time.time;
        private void OnJumpPerformed(InputAction.CallbackContext _) => lastJumpPressedTime = Time.time;
        private void OnNextWeaponPerformed(InputAction.CallbackContext _) => lastNextWeaponPressedTime = Time.time;
        private void OnPreviousWeaponPerformed(InputAction.CallbackContext _) => lastPreviousWeaponPressedTime = Time.time;
    }
}
