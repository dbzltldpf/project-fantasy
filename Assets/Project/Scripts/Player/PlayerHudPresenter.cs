using ProjectFantasy.InventorySystem;
using ProjectFantasy.Items;
using ProjectFantasy.UI;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 플레이어 상태를 HUD에 전달: 퀵슬롯 바, 화살 수, 줍기 안내, 안내 문구
    [DisallowMultipleComponent]
    public sealed class PlayerHudPresenter : MonoBehaviour
    {
        [Tooltip("플레이어")]
        [SerializeField] private PlayerController player;
        [Tooltip("퀵슬롯 바 UI")]
        [SerializeField] private QuickSlotBarView quickSlotBar;
        [Tooltip("화살 수 UI")]
        [SerializeField] private AmmoCounterView ammoCounter;
        [Tooltip("줍기 안내 UI")]
        [SerializeField] private InteractPromptView interactPrompt;
        [Tooltip("안내 문구 UI")]
        [SerializeField] private NoticeView notice;

        private PlayerItemHandler itemHandler;
        private PlayerLoadout loadout;
        private PlayerRangedWeapon rangedWeapon;
        private Inventory inventory;
        private PlayerInteractor interactor;

        // 다른 오브젝트의 Awake 순서에 의존하지 않도록 직접 조회
        private void Awake()
        {
            itemHandler = player.GetComponent<PlayerItemHandler>();
            loadout = player.GetComponent<PlayerLoadout>();
            rangedWeapon = player.GetComponent<PlayerRangedWeapon>();
            inventory = player.GetComponent<Inventory>();
            interactor = player.GetComponent<PlayerInteractor>();
        }

        private void OnEnable()
        {
            loadout.WeaponChanged += HandleWeaponChanged;
            rangedWeapon.LoadedChanged += HandleLoadedChanged;
            inventory.Changed += RefreshAmmo;
            interactor.TargetChanged += HandleInteractTargetChanged;
            itemHandler.Noticed += notice.Show;
        }

        private void OnDisable()
        {
            loadout.WeaponChanged -= HandleWeaponChanged;
            rangedWeapon.LoadedChanged -= HandleLoadedChanged;
            inventory.Changed -= RefreshAmmo;
            interactor.TargetChanged -= HandleInteractTargetChanged;
            itemHandler.Noticed -= notice.Show;
        }

        private void Start()
        {
            quickSlotBar.Bind(itemHandler.QuickSlots, inventory, itemHandler);
            RefreshAmmo();
        }

        private void HandleWeaponChanged(WeaponData _) => RefreshAmmo();
        private void HandleLoadedChanged(bool _) => RefreshAmmo();

        // 원거리 무기일 때만 화살 수 표시 (석궁은 장전 상태 포함)
        private void RefreshAmmo()
        {
            RangedWeaponData weapon = rangedWeapon.Current;
            if (weapon == null)
            {
                ammoCounter.Hide();
                return;
            }

            ammoCounter.Show(inventory.GetCount(weapon.Ammo), weapon.RequiresReload, rangedWeapon.IsLoaded);
        }

        private void HandleInteractTargetChanged(WorldItem target)
        {
            if (target != null && target.Item != null) interactPrompt.Show(target.Item.DisplayName, target.Count);
            else interactPrompt.Hide();
        }
    }
}
