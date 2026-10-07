using System;
using ProjectFantasy.Combat;
using ProjectFantasy.Combat;
using ProjectFantasy.InventorySystem;
using ProjectFantasy.Items;
using ProjectFantasy.Mastery;
using ProjectFantasy.UI;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 플레이어 상태를 HUD에 전달: 체력 바, 퀵슬롯 바, 화살 수, 줍기 안내, 안내 문구(아이템·숙련도)
    [DisallowMultipleComponent]
    public sealed class PlayerHudPresenter : MonoBehaviour
    {
        [Tooltip("플레이어")]
        [SerializeField] private PlayerController player;
        [Tooltip("체력 바 UI (Create Player Status UI로 생성, 비어 있으면 무시)")]
        [SerializeField] private StatusBarView healthBar;
        [Tooltip("퀵슬롯 바 UI")]
        [SerializeField] private QuickSlotBarView quickSlotBar;
        [Tooltip("화살 수 UI")]
        [SerializeField] private AmmoCounterView ammoCounter;
        [Tooltip("줍기 안내 UI")]
        [SerializeField] private InteractPromptView interactPrompt;
        [Tooltip("안내 문구 UI")]
        [SerializeField] private NoticeView notice;

        [Header("Mastery Messages")]
        [Tooltip("{0} = 숙련 이름, {1} = 경험치")]
        [SerializeField] private string experienceFormat = "{0} 숙련 +{1}";
        [Tooltip("{0} = 숙련 이름, {1} = 레벨")]
        [SerializeField] private string levelUpFormat = "{0} 숙련 Lv {1}";
        [Tooltip("{0} = 숙련 이름, {1} = 레벨, {2} = 해금 티어")]
        [SerializeField] private string tierUnlockFormat = "{0} 숙련 Lv {1} — T{2} 장착 가능";

        private Health health;
        private PlayerItemHandler itemHandler;
        private PlayerLoadout loadout;
        private PlayerRangedWeapon rangedWeapon;
        private Inventory inventory;
        private PlayerInteractor interactor;
        private WeaponMastery mastery;
        private int[] unlockedTiers = Array.Empty<int>();

        // 다른 오브젝트의 Awake 순서에 의존하지 않도록 직접 조회
        private void Awake()
        {
            health = player.GetComponent<Health>();
            itemHandler = player.GetComponent<PlayerItemHandler>();
            loadout = player.GetComponent<PlayerLoadout>();
            rangedWeapon = player.GetComponent<PlayerRangedWeapon>();
            inventory = player.GetComponent<Inventory>();
            interactor = player.GetComponent<PlayerInteractor>();
            mastery = player.GetComponent<WeaponMastery>();
        }

        private void OnEnable()
        {
            health.HealthChanged += HandleHealthChanged;
            loadout.WeaponChanged += HandleWeaponChanged;
            rangedWeapon.LoadedChanged += HandleLoadedChanged;
            inventory.Changed += RefreshAmmo;
            interactor.TargetChanged += HandleInteractTargetChanged;
            itemHandler.Noticed += notice.Show;
            mastery.ExperienceGained += HandleExperienceGained;
            mastery.LevelChanged += HandleLevelChanged;
        }

        private void OnDisable()
        {
            health.HealthChanged -= HandleHealthChanged;
            loadout.WeaponChanged -= HandleWeaponChanged;
            rangedWeapon.LoadedChanged -= HandleLoadedChanged;
            inventory.Changed -= RefreshAmmo;
            interactor.TargetChanged -= HandleInteractTargetChanged;
            itemHandler.Noticed -= notice.Show;
            mastery.ExperienceGained -= HandleExperienceGained;
            mastery.LevelChanged -= HandleLevelChanged;
        }

        private void Start()
        {
            HandleHealthChanged(health.CurrentHealth, health.MaxHealth);
            quickSlotBar.Bind(itemHandler.QuickSlots, inventory, itemHandler);
            RefreshAmmo();
            CacheUnlockedTiers();
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (healthBar != null) healthBar.SetValue(current, max);
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

        // 티어 해금 알림 판정용 현재 해금 티어 기록
        private void CacheUnlockedTiers()
        {
            MasteryType[] types = (MasteryType[])Enum.GetValues(typeof(MasteryType));
            unlockedTiers = new int[types.Length];
            foreach (MasteryType type in types)
            {
                unlockedTiers[(int)type] = mastery.GetUnlockedTier(type);
            }
        }

        private void HandleExperienceGained(MasteryType type, int amount)
        {
            notice.ShowMessage(string.Format(experienceFormat, mastery.Data.GetDisplayName(type), amount));
        }

        // 새 티어가 열리면 장착 가능 안내 포함
        private void HandleLevelChanged(MasteryType type, int level)
        {
            string masteryName = mastery.Data.GetDisplayName(type);
            int tier = mastery.GetUnlockedTier(type);
            bool isNewTier = tier > unlockedTiers[(int)type];
            unlockedTiers[(int)type] = tier;

            notice.ShowMessage(isNewTier ? string.Format(tierUnlockFormat, masteryName, level, tier) : string.Format(levelUpFormat, masteryName, level));
        }

        private void HandleInteractTargetChanged(WorldItem target)
        {
            if (target != null && target.Item != null) interactPrompt.Show(target.Instance?.DisplayName ?? target.Item.DisplayName, target.Count);
            else interactPrompt.Hide();
        }
    }
}
