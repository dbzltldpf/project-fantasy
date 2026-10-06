using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectFantasy.Player
{
    // 입력 수집과 선입력 버퍼링 전담 (액션 에셋에서 이름으로 탐색, 메뉴가 열리면 게임플레이 입력 차단)
    [DisallowMultipleComponent]
    public sealed class PlayerInputHandler : MonoBehaviour
    {
        public const int NoQuickSlot = -1;

        private const string PlayerMapName = "Player";
        private const string MoveName = "Move";
        private const string AttackName = "Attack";
        private const string JumpName = "Jump";
        private const string SprintName = "Sprint";
        private const string SecondaryName = "Secondary";
        private const string InteractName = "Interact";
        private const string InventoryName = "Inventory";
        private const string QuickSlotName = "QuickSlot";

        [Tooltip("InputSystem_Actions (Player 맵의 액션을 이름으로 사용)")]
        [SerializeField] private InputActionAsset actionAsset;
        [Tooltip("공격·점프·줍기·퀵슬롯 입력을 이 시간(초) 동안 보관해 동작이 끝나면 실행")]
        [SerializeField, Min(0f)] private float inputBufferTime = 0.2f;

        private InputActionMap playerMap;
        private InputAction moveAction;
        private InputAction attackAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction secondaryAction;
        private InputAction interactAction;
        private InputAction inventoryAction;
        private InputAction quickSlotAction;

        private float lastAttackPressedTime = float.NegativeInfinity;
        private float lastJumpPressedTime = float.NegativeInfinity;
        private float lastInteractPressedTime = float.NegativeInfinity;
        private float lastQuickSlotPressedTime = float.NegativeInfinity;
        private int pendingQuickSlot = NoQuickSlot;
        private bool isGameplayEnabled = true;

        public Vector2 MoveInput => isGameplayEnabled ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        public bool IsSprintHeld => isGameplayEnabled && sprintAction.IsPressed();
        public bool IsSecondaryHeld => isGameplayEnabled && secondaryAction.IsPressed();

        // 메뉴 토글은 게임플레이 차단 중에도 동작
        public event Action InventoryPressed;

        // 이름이 틀리면 throwIfNotFound로 즉시 원인 표시
        private void Awake()
        {
            playerMap = actionAsset.FindActionMap(PlayerMapName, true);
            moveAction = playerMap.FindAction(MoveName, true);
            attackAction = playerMap.FindAction(AttackName, true);
            jumpAction = playerMap.FindAction(JumpName, true);
            sprintAction = playerMap.FindAction(SprintName, true);
            secondaryAction = playerMap.FindAction(SecondaryName, true);
            interactAction = playerMap.FindAction(InteractName, true);
            inventoryAction = playerMap.FindAction(InventoryName, true);
            quickSlotAction = playerMap.FindAction(QuickSlotName, true);
        }

        private void OnEnable()
        {
            attackAction.performed += OnAttackPerformed;
            jumpAction.performed += OnJumpPerformed;
            interactAction.performed += OnInteractPerformed;
            inventoryAction.performed += OnInventoryPerformed;
            quickSlotAction.performed += OnQuickSlotPerformed;
            playerMap.Enable();
        }

        private void OnDisable()
        {
            attackAction.performed -= OnAttackPerformed;
            jumpAction.performed -= OnJumpPerformed;
            interactAction.performed -= OnInteractPerformed;
            inventoryAction.performed -= OnInventoryPerformed;
            quickSlotAction.performed -= OnQuickSlotPerformed;
            ClearBuffers();
        }

        public void SetGameplayEnabled(bool isEnabled)
        {
            isGameplayEnabled = isEnabled;
            if (!isEnabled) ClearBuffers();
        }

        public bool ConsumeAttack() => ConsumeBuffered(ref lastAttackPressedTime);
        public bool ConsumeJump() => ConsumeBuffered(ref lastJumpPressedTime);
        public bool ConsumeInteract() => ConsumeBuffered(ref lastInteractPressedTime);

        // 버퍼된 퀵슬롯 번호 (없으면 NoQuickSlot)
        public int ConsumeQuickSlot()
        {
            int slot = pendingQuickSlot;
            pendingQuickSlot = NoQuickSlot;
            return ConsumeBuffered(ref lastQuickSlotPressedTime) ? slot : NoQuickSlot;
        }

        public void ClearBuffers()
        {
            lastAttackPressedTime = float.NegativeInfinity;
            lastJumpPressedTime = float.NegativeInfinity;
            lastInteractPressedTime = float.NegativeInfinity;
            lastQuickSlotPressedTime = float.NegativeInfinity;
            pendingQuickSlot = NoQuickSlot;
        }

        private bool ConsumeBuffered(ref float pressedTime)
        {
            if (Time.time - pressedTime > inputBufferTime) return false;

            pressedTime = float.NegativeInfinity;
            return true;
        }

        private void BufferQuickSlot(int slot)
        {
            pendingQuickSlot = slot;
            lastQuickSlotPressedTime = Time.time;
        }

        private void OnAttackPerformed(InputAction.CallbackContext _)
        {
            if (isGameplayEnabled) lastAttackPressedTime = Time.time;
        }

        private void OnJumpPerformed(InputAction.CallbackContext _)
        {
            if (isGameplayEnabled) lastJumpPressedTime = Time.time;
        }

        private void OnInteractPerformed(InputAction.CallbackContext _)
        {
            if (isGameplayEnabled) lastInteractPressedTime = Time.time;
        }

        private void OnInventoryPerformed(InputAction.CallbackContext _) => InventoryPressed?.Invoke();

        // 눌린 키의 바인딩 순서가 슬롯 번호
        private void OnQuickSlotPerformed(InputAction.CallbackContext context)
        {
            if (!isGameplayEnabled) return;

            int bindingIndex = context.action.GetBindingIndexForControl(context.control);
            if (bindingIndex >= 0) BufferQuickSlot(bindingIndex);
        }
    }
}
