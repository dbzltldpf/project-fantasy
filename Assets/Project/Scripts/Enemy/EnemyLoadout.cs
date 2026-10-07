using ProjectFantasy.Combat;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 적 장비 적용: 무기·방패 모델, 근접 판정(칼날/몸 기준), 방패 방어력
    [RequireComponent(typeof(EquipmentVisual), typeof(MeleeAttacker), typeof(ShieldGuard))]
    [DisallowMultipleComponent]
    public sealed class EnemyLoadout : MonoBehaviour
    {
        private EquipmentVisual equipmentVisual;
        private MeleeAttacker attacker;
        private ShieldGuard shieldGuard;

        public ShieldGuard ShieldGuard => shieldGuard;

        public void Apply(EnemyData data)
        {
            equipmentVisual = GetComponent<EquipmentVisual>();
            attacker = GetComponent<MeleeAttacker>();
            shieldGuard = GetComponent<ShieldGuard>();

            ApplyWeapon(data.Weapon);
            ApplyShield(data);
        }

        private void ApplyWeapon(WeaponData weapon)
        {
            if (weapon == null) return;

            equipmentVisual.Show(weapon.GripHand, weapon);
            attacker.SetHitShape(weapon.HitOffset, weapon.HitRadius);

            Transform model = equipmentVisual.GetActiveModel(weapon.GripHand);
            if (weapon.HasBlade && model != null) attacker.SetBlade(model, weapon.BladeBase, weapon.BladeTip, weapon.BladeRadius);
            else attacker.ClearBlade();
        }

        private void ApplyShield(EnemyData data)
        {
            if (!data.HasShield)
            {
                shieldGuard.DisableShield();
                return;
            }

            equipmentVisual.Show(EquipHand.Left, data.Shield);
            shieldGuard.EnableShield(data.Shield.GuardAngle, data.Defense, data.MagicDefense);
        }
    }
}
