using System.Collections.Generic;
using ProjectFantasy.Items;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 활 시위에 건 화살(오른손), 석궁에 장전된 볼트 표시
    [RequireComponent(typeof(EquipmentVisual), typeof(PlayerLoadout), typeof(PlayerRangedWeapon))]
    [DisallowMultipleComponent]
    public sealed class PlayerAmmoVisual : MonoBehaviour
    {
        private readonly Dictionary<AmmoData, Transform> loadedAmmoCache = new Dictionary<AmmoData, Transform>();
        private EquipmentVisual equipmentVisual;
        private PlayerLoadout loadout;
        private PlayerRangedWeapon rangedWeapon;
        private Transform activeLoadedAmmo;

        private void Awake()
        {
            equipmentVisual = GetComponent<EquipmentVisual>();
            loadout = GetComponent<PlayerLoadout>();
            rangedWeapon = GetComponent<PlayerRangedWeapon>();
        }

        private void OnEnable()
        {
            loadout.WeaponChanged += HandleWeaponChanged;
            rangedWeapon.LoadedChanged += HandleLoadedChanged;
        }

        private void OnDisable()
        {
            loadout.WeaponChanged -= HandleWeaponChanged;
            rangedWeapon.LoadedChanged -= HandleLoadedChanged;
        }

        // 왼손 활일 때만 비어 있는 오른손에 화살 표시
        public void SetNocked(bool isNocked)
        {
            RangedWeaponData weapon = rangedWeapon.Current;
            if (weapon == null || !weapon.ShowsNockedAmmo || !weapon.IsHeldInLeftHand) return;

            if (isNocked) equipmentVisual.Show(EquipHand.Right, weapon.Ammo);
            else equipmentVisual.Hide(EquipHand.Right);
        }

        private void HandleWeaponChanged(WeaponData _) => RefreshLoadedAmmo();
        private void HandleLoadedChanged(bool _) => RefreshLoadedAmmo();

        private void RefreshLoadedAmmo()
        {
            if (activeLoadedAmmo != null) activeLoadedAmmo.gameObject.SetActive(false);
            activeLoadedAmmo = null;

            RangedWeaponData weapon = rangedWeapon.Current;
            if (weapon == null || !weapon.RequiresReload || !rangedWeapon.IsLoaded) return;

            Transform weaponModel = equipmentVisual.GetActiveModel(weapon.GripHand);
            activeLoadedAmmo = GetOrCreateLoadedAmmo(weapon.Ammo);
            if (weaponModel == null || activeLoadedAmmo == null) return;

            activeLoadedAmmo.SetParent(weaponModel, false);
            activeLoadedAmmo.SetLocalPositionAndRotation(weapon.LoadedAmmoPosition, weapon.LoadedAmmoRotation);
            activeLoadedAmmo.gameObject.SetActive(true);
        }

        private Transform GetOrCreateLoadedAmmo(AmmoData ammo)
        {
            if (ammo == null || ammo.ModelPrefab == null) return null;
            if (loadedAmmoCache.TryGetValue(ammo, out Transform cached)) return cached;

            Transform instance = Instantiate(ammo.ModelPrefab, transform).transform;
            ammo.ApplyModelVisual(instance.gameObject, ItemData.MinTier);
            instance.gameObject.SetActive(false);
            loadedAmmoCache.Add(ammo, instance);
            return instance;
        }
    }
}
