using System;
using System.Collections.Generic;
using ProjectFantasy.Combat;
using ProjectFantasy.Weapon;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectFantasy.Player
{
    // 보유 무기/보조 장비 관리와 장착 적용 (모델, 판정 범위, 가드)
    [RequireComponent(typeof(EquipmentVisual), typeof(MeleeAttacker), typeof(ShieldGuard))]
    [DisallowMultipleComponent]
    public sealed class PlayerLoadout : MonoBehaviour
    {
        private const int UnarmedIndex = -1;
        private const int FirstWeaponIndex = 0;
        private const int MinCycleCount = 2;

        [SerializeField] private WeaponData unarmedWeapon;
        [SerializeField] private WeaponData[] startingWeapons = Array.Empty<WeaponData>();
        [SerializeField, FormerlySerializedAs("startingShield")] private OffHandData startingOffHand;

        private readonly List<WeaponData> ownedWeapons = new List<WeaponData>();
        private EquipmentVisual equipmentVisual;
        private MeleeAttacker attacker;
        private ShieldGuard shieldGuard;
        private OffHandData currentOffHand;
        private int currentIndex = UnarmedIndex;

        public WeaponData CurrentWeapon { get; private set; }
        public OffHandData CurrentOffHand => currentOffHand;
        public ShieldData CurrentShield => currentOffHand as ShieldData;
        public WeaponData UnarmedWeapon => unarmedWeapon;
        public IReadOnlyList<WeaponData> OwnedWeapons => ownedWeapons;
        public bool CanGuard => shieldGuard.HasShield;

        public event Action<WeaponData> WeaponChanged;

        private void Awake()
        {
            equipmentVisual = GetComponent<EquipmentVisual>();
            attacker = GetComponent<MeleeAttacker>();
            shieldGuard = GetComponent<ShieldGuard>();

            foreach (WeaponData weapon in startingWeapons)
            {
                if (weapon == null) continue;
                ownedWeapons.Add(weapon);
                equipmentVisual.Preload(weapon, weapon.GripHand);
            }

            currentOffHand = startingOffHand;
            equipmentVisual.Preload(currentOffHand, EquipHand.Left);
        }

        private void Start()
        {
            EquipIndex(ownedWeapons.Count > 0 ? FirstWeaponIndex : UnarmedIndex);
        }

        // direction: +1 다음, -1 이전
        public void CycleWeapon(int direction)
        {
            int count = ownedWeapons.Count;
            if (count < MinCycleCount) return;

            int nextIndex = ((currentIndex + direction) % count + count) % count;
            EquipIndex(nextIndex);
        }

        private void EquipIndex(int index)
        {
            currentIndex = index;
            CurrentWeapon = index == UnarmedIndex ? unarmedWeapon : ownedWeapons[index];
            if (CurrentWeapon == null) return;

            equipmentVisual.Show(CurrentWeapon.GripHand, CurrentWeapon);
            if (CurrentWeapon.IsHeldInLeftHand) equipmentVisual.Hide(EquipHand.Right);

            attacker.SetHitShape(CurrentWeapon.HitOffset, CurrentWeapon.HitRadius);
            RefreshOffHand();

            WeaponChanged?.Invoke(CurrentWeapon);
        }

        // 무기와 함께 들 수 없는 보조 장비는 숨김, 방패일 때만 가드 활성화
        private void RefreshOffHand()
        {
            bool canEquip = currentOffHand != null && currentOffHand.CanEquipWith(CurrentWeapon);

            if (canEquip) equipmentVisual.Show(EquipHand.Left, currentOffHand);
            else if (!CurrentWeapon.IsHeldInLeftHand) equipmentVisual.Hide(EquipHand.Left);

            if (canEquip && currentOffHand is ShieldData shield) shieldGuard.EnableShield(shield.GuardAngle);
            else shieldGuard.DisableShield();
        }
    }
}
