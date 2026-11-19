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
        [Tooltip("숙련도 창 UI")]
        [SerializeField] private MasteryWindow masteryWindow;
        [Tooltip("일시정지 중이면 Tab·K 무시")]
        [SerializeField] private PausePresenter pausePresenter;
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
            inputHandler.InventoryPressed += HandleInventoryPressed;
            inventoryWindow.OpenChanged += HandleOpenChanged;
            inputHandler.MasteryPressed += HandleMasteryPressed;
            masteryWindow.OpenChanged += HandleOpenChanged;
        }

        private void OnDisable()
        {
            inputHandler.InventoryPressed -= HandleInventoryPressed;
            inventoryWindow.OpenChanged -= HandleOpenChanged;
            inputHandler.MasteryPressed -= HandleMasteryPressed;
            masteryWindow.OpenChanged -= HandleOpenChanged;
        }

        private void Start()
        {
            inventoryWindow.Bind(itemHandler.Inventory, itemHandler);
            masteryWindow.Bind(mastery);
        }

        private void HandleOpenChanged(bool _)
        {
            bool isAnyOpen = inventoryWindow.IsOpen || masteryWindow.IsOpen;
            inputHandler.SetGameplayEnabled(!isAnyOpen);
            thirdPersonCamera.SetLookEnabled(!isAnyOpen);
        }
        private void HandleInventoryPressed()
        {
            if (pausePresenter.IsPaused) return;
            inventoryWindow.Toggle();
        }
        private void HandleMasteryPressed()
        {
            if (pausePresenter.IsPaused) return;
            masteryWindow.Toggle();
        }

    }
}
