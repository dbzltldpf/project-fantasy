using ProjectFantasy.CameraSystem;
using ProjectFantasy.Mastery;
using ProjectFantasy.UI;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 인벤토리(Tab)·숙련도(K) 창 토글 → 하나라도 열리면 게임플레이 입력 차단, 시점 정지, 커서 표시 (실시간이라 피격은 그대로)
    [DisallowMultipleComponent]
    public sealed class PlayerMenuPresenter : MonoBehaviour
    {
        [Tooltip("플레이어")]
        [SerializeField] private PlayerController player;
        [Tooltip("3인칭 카메라 (메뉴 중 시점 정지)")]
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [Tooltip("인벤토리 창 UI")]
        [SerializeField] private InventoryWindow inventoryWindow;
        [Tooltip("숙련도 창 UI (비우면 K 키 무시)")]
        [SerializeField] private MasteryWindow masteryWindow;

        private PlayerInputHandler inputHandler;
        private PlayerItemHandler itemHandler;
        private WeaponMastery mastery;

        // 다른 오브젝트의 Awake 순서에 의존하지 않도록 직접 조회
        private void Awake()
        {
            inputHandler = player.GetComponent<PlayerInputHandler>();
            itemHandler = player.GetComponent<PlayerItemHandler>();
            mastery = player.GetComponent<WeaponMastery>();
        }

        private void OnEnable()
        {
            inputHandler.InventoryPressed += inventoryWindow.Toggle;
            inventoryWindow.OpenChanged += HandleOpenChanged;
            if (masteryWindow == null) return;

            inputHandler.MasteryPressed += masteryWindow.Toggle;
            masteryWindow.OpenChanged += HandleOpenChanged;
        }

        private void OnDisable()
        {
            inputHandler.InventoryPressed -= inventoryWindow.Toggle;
            inventoryWindow.OpenChanged -= HandleOpenChanged;
            if (masteryWindow == null) return;

            inputHandler.MasteryPressed -= masteryWindow.Toggle;
            masteryWindow.OpenChanged -= HandleOpenChanged;
        }

        private void Start()
        {
            inventoryWindow.Bind(itemHandler.Inventory, itemHandler);
            if (masteryWindow != null) masteryWindow.Bind(mastery);
        }

        private void HandleOpenChanged(bool _)
        {
            bool isAnyOpen = inventoryWindow.IsOpen || (masteryWindow != null && masteryWindow.IsOpen);
            inputHandler.SetGameplayEnabled(!isAnyOpen);
            thirdPersonCamera.SetLookEnabled(!isAnyOpen);
        }
    }
}
