using ProjectFantasy.CameraSystem;
using ProjectFantasy.UI;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 인벤토리 창 열기/닫기 → 게임플레이 입력 차단, 시점 정지, 커서 표시 (실시간이라 피격은 그대로)
    [DisallowMultipleComponent]
    public sealed class PlayerMenuPresenter : MonoBehaviour
    {
        [Tooltip("플레이어")]
        [SerializeField] private PlayerController player;
        [Tooltip("3인칭 카메라 (메뉴 중 시점 정지)")]
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [Tooltip("인벤토리 창 UI")]
        [SerializeField] private InventoryWindow inventoryWindow;

        private PlayerInputHandler inputHandler;
        private PlayerItemHandler itemHandler;

        // 다른 오브젝트의 Awake 순서에 의존하지 않도록 직접 조회
        private void Awake()
        {
            inputHandler = player.GetComponent<PlayerInputHandler>();
            itemHandler = player.GetComponent<PlayerItemHandler>();
        }

        private void OnEnable()
        {
            inputHandler.InventoryPressed += inventoryWindow.Toggle;
            inventoryWindow.OpenChanged += HandleOpenChanged;
        }

        private void OnDisable()
        {
            inputHandler.InventoryPressed -= inventoryWindow.Toggle;
            inventoryWindow.OpenChanged -= HandleOpenChanged;
        }

        private void Start() => inventoryWindow.Bind(itemHandler.Inventory, itemHandler);

        private void HandleOpenChanged(bool isOpen)
        {
            inputHandler.SetGameplayEnabled(!isOpen);
            thirdPersonCamera.SetLookEnabled(!isOpen);
        }
    }
}
