using System;
using ProjectFantasy.Combat;
using ProjectFantasy.InventorySystem;
using ProjectFantasy.Items;
using ProjectFantasy.Mastery;
using ProjectFantasy.Weapon;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectFantasy.Player
{
    // 장착 무기·보조 장비 개체 적용 (모델, 판정, 가드·방어력, 숙련 보너스), 무기와 맞지 않는 보조 장비는 비활성 유지
    [RequireComponent(typeof(EquipmentVisual), typeof(MeleeAttacker), typeof(ShieldGuard))]
    [RequireComponent(typeof(Inventory), typeof(WeaponMastery))]
    [DisallowMultipleComponent]
    public sealed class PlayerLoadout : MonoBehaviour
    {
        [Tooltip("무기가 없을 때 쓰는 맨손 데이터 (필수)")]
        [SerializeField] private WeaponData unarmedWeapon;
        [Tooltip("시작 시 장착할 무기 종류 (가방의 첫 개체)")]
        [SerializeField] private WeaponData startingWeapon;
        [Tooltip("시작 시 장착할 보조 장비 종류 (가방의 첫 개체)")]
        [SerializeField, FormerlySerializedAs("startingShield")] private OffHandData startingOffHand;

        private EquipmentVisual equipmentVisual;
        private MeleeAttacker attacker;
        private ShieldGuard shieldGuard;
        private Inventory inventory;
        private WeaponMastery mastery;

        public ItemInstance WeaponInstance { get; private set; }
        public ItemInstance OffHandInstance { get; private set; }
        public WeaponData CurrentWeapon { get; private set; }
        public OffHandData CurrentOffHand => OffHandInstance?.Data as OffHandData;
        public bool IsOffHandActive { get; private set; }
        // 효과가 적용 중인 보조 장비만 (비활성이면 null)
        public OffHandData ActiveOffHand => IsOffHandActive ? CurrentOffHand : null;
        public ShieldData ActiveShield => ActiveOffHand as ShieldData;
        public WeaponData UnarmedWeapon => unarmedWeapon;
        public bool IsUnarmed => WeaponInstance == null;
        public bool CanGuard => shieldGuard.HasShield;

        // 맨손은 데이터 기본값
        public ItemStats WeaponStats => WeaponInstance != null ? WeaponInstance.Stats : unarmedWeapon.BaseStats;

        // 무기 숙련 보너스 적용 (피해 계산용)
        public float AttackPower => WeaponStats.AttackPower * mastery.GetBonusMultiplier(CurrentWeapon);
        public float MagicPower => WeaponStats.MagicPower * mastery.GetBonusMultiplier(CurrentWeapon);

        public event Action<WeaponData> WeaponChanged;
        public event Action<OffHandData> OffHandChanged;
        // 무기 교체로 보조 장비가 활성↔비활성 전환될 때
        public event Action<bool> OffHandActiveChanged;

        private void Awake()
        {
            equipmentVisual = GetComponent<EquipmentVisual>();
            attacker = GetComponent<MeleeAttacker>();
            shieldGuard = GetComponent<ShieldGuard>();
            inventory = GetComponent<Inventory>();
            mastery = GetComponent<WeaponMastery>();
        }

        private void OnEnable()
        {
            inventory.Changed += HandleInventoryChanged;
            mastery.LevelChanged += HandleMasteryLevelChanged;
        }

        private void OnDisable()
        {
            inventory.Changed -= HandleInventoryChanged;
            mastery.LevelChanged -= HandleMasteryLevelChanged;
        }

        // 가방 개체가 만들어진 뒤(Awake 이후) 시작 장비 장착
        private void Start()
        {
            PreloadInventoryModels();
            OffHandInstance = FindStartingInstance(startingOffHand);
            EquipWeapon(FindStartingInstance(startingWeapon));
        }

        // 숙련도가 부족한 시작 장비는 장착하지 않음
        private ItemInstance FindStartingInstance(ItemData item)
        {
            ItemInstance instance = item != null ? inventory.FindFirstInstance(item) : null;
            return mastery.CanUse(instance) ? instance : null;
        }

        public bool IsEquipped(ItemInstance instance) => instance != null && (instance == WeaponInstance || instance == OffHandInstance);

        public bool CanEquipOffHand(OffHandData offHand) => offHand != null && offHand.CanEquipWith(CurrentWeapon);

        // null이면 맨손
        public void EquipWeapon(ItemInstance instance)
        {
            WeaponData weapon = instance?.Data as WeaponData;
            WeaponInstance = weapon != null ? instance : null;
            CurrentWeapon = weapon != null ? weapon : unarmedWeapon;
            if (CurrentWeapon == null) return;

            equipmentVisual.Show(CurrentWeapon.GripHand, CurrentWeapon, WeaponInstance?.Tier ?? ItemData.MinTier);
            if (CurrentWeapon.IsHeldInLeftHand) equipmentVisual.Hide(EquipHand.Right);

            ApplyHitShape();
            RefreshOffHand(true);

            WeaponChanged?.Invoke(CurrentWeapon);
        }

        public void EquipOffHand(ItemInstance instance)
        {
            OffHandInstance = instance?.Data is OffHandData ? instance : null;
            RefreshOffHand(false);
            OffHandChanged?.Invoke(CurrentOffHand);
        }

        public void Unequip(ItemInstance instance)
        {
            if (instance == null) return;

            if (instance == WeaponInstance) EquipWeapon(null);
            else if (instance == OffHandInstance) EquipOffHand(null);
        }

        // 칼날 정보가 있으면 무기 모델 궤적 판정, 없으면 몸 기준 구체 판정
        private void ApplyHitShape()
        {
            attacker.SetHitShape(CurrentWeapon.HitOffset, CurrentWeapon.HitRadius);

            Transform model = equipmentVisual.GetActiveModel(CurrentWeapon.GripHand);
            if (CurrentWeapon.HasBlade && model != null)
            {
                attacker.SetBlade(model, CurrentWeapon.BladeBase, CurrentWeapon.BladeTip, CurrentWeapon.BladeRadius);
            }
            else
            {
                attacker.ClearBlade();
            }
        }

        // 무기와 맞지 않는 보조 장비는 숨기고 효과 해제(장착은 유지), 방패면 가드·방어력 적용
        private void RefreshOffHand(bool notifyActiveChange)
        {
            if (CurrentWeapon == null) return;

            OffHandData offHand = CurrentOffHand;
            bool isActive = offHand != null && offHand.CanEquipWith(CurrentWeapon);
            bool wasActive = IsOffHandActive;
            IsOffHandActive = isActive;

            if (isActive) equipmentVisual.Show(EquipHand.Left, offHand, OffHandInstance.Tier);
            else if (!CurrentWeapon.IsHeldInLeftHand) equipmentVisual.Hide(EquipHand.Left);

            if (isActive && offHand is ShieldData shield)
            {
                ItemStats stats = OffHandInstance.Stats;
                float bonus = mastery.GetBonusMultiplier(shield);
                shieldGuard.EnableShield(shield.GuardAngle, Mathf.RoundToInt(stats.Defense * bonus), Mathf.RoundToInt(stats.MagicDefense * bonus));
            }
            else
            {
                shieldGuard.DisableShield();
            }

            if (notifyActiveChange && offHand != null && wasActive != isActive) OffHandActiveChanged?.Invoke(isActive);
        }

        // 방패 숙련 레벨이 오르면 방어력 즉시 갱신
        private void HandleMasteryLevelChanged(MasteryType type, int _)
        {
            if (type == MasteryType.Shield && ActiveShield != null) RefreshOffHand(false);
        }

        private void HandleInventoryChanged()
        {
            PreloadInventoryModels();

            if (WeaponInstance != null && !inventory.Contains(WeaponInstance)) EquipWeapon(null);
            if (OffHandInstance != null && !inventory.Contains(OffHandInstance)) EquipOffHand(null);
        }

        // 장착 전에 모델을 미리 생성해 교체 시 Instantiate 방지 (이미 생성된 모델은 캐시 조회만)
        private void PreloadInventoryModels()
        {
            for (int i = 0; i < inventory.Capacity; i++)
            {
                ItemInstance instance = inventory.GetSlot(i).Instance;
                if (instance?.Data is WeaponData weapon) equipmentVisual.Preload(weapon, weapon.GripHand, instance.Tier);
                else if (instance?.Data is OffHandData offHand) equipmentVisual.Preload(offHand, EquipHand.Left, instance.Tier);
            }
        }
    }
}
