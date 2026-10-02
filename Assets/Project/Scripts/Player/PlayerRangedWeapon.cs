using System;
using System.Collections.Generic;
using ProjectFantasy.Combat;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 원거리 무기 발사/조준/장전 가능 여부와 화살 소모 규칙
    [RequireComponent(typeof(PlayerLoadout), typeof(AmmoPouch))]
    [DisallowMultipleComponent]
    public sealed class PlayerRangedWeapon : MonoBehaviour
    {
        private readonly HashSet<RangedWeaponData> loadedWeapons = new HashSet<RangedWeaponData>();
        private PlayerLoadout loadout;
        private AmmoPouch ammoPouch;

        public RangedWeaponData Current => loadout.CurrentWeapon as RangedWeaponData;
        public bool IsEquipped => Current != null;
        public bool IsLoaded => IsEquipped && loadedWeapons.Contains(Current);

        // 활: 화살 1개 이상, 석궁: 장전 완료
        public bool CanFire => IsEquipped && (Current.RequiresReload ? IsLoaded : ammoPouch.HasAmmo(Current.Ammo));
        public bool CanAim => CanFire && Current.CanAim;
        public bool NeedsReload => IsEquipped && Current.RequiresReload && !IsLoaded && ammoPouch.HasAmmo(Current.Ammo);

        public event Action<bool> LoadedChanged;

        private void Awake()
        {
            loadout = GetComponent<PlayerLoadout>();
            ammoPouch = GetComponent<AmmoPouch>();
        }

        // 현재 무기 + 화살 데이터로 투사체 설정 조합
        public bool TryGetProjectileProfile(out ProjectileProfile profile)
        {
            RangedWeaponData weapon = Current;
            if (weapon == null || weapon.Ammo == null)
            {
                profile = default;
                return false;
            }

            profile = weapon.Ammo.ToProfile(weapon.LaunchSpeed);
            return true;
        }

        // 활은 화살 소모, 석궁은 장전된 볼트 사용
        public bool TryConsumeShot()
        {
            RangedWeaponData weapon = Current;
            if (weapon == null) return false;
            if (!weapon.RequiresReload) return ammoPouch.TryConsume(weapon.Ammo);
            if (!loadedWeapons.Remove(weapon)) return false;

            LoadedChanged?.Invoke(false);
            return true;
        }

        // 장전 완료 시 화살 1개를 석궁에 장착
        public bool TryCompleteReload()
        {
            RangedWeaponData weapon = Current;
            if (weapon == null || !weapon.RequiresReload || IsLoaded) return false;
            if (!ammoPouch.TryConsume(weapon.Ammo)) return false;

            loadedWeapons.Add(weapon);
            LoadedChanged?.Invoke(true);
            return true;
        }
    }
}
