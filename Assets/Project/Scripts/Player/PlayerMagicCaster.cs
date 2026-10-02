using System.Collections.Generic;
using ProjectFantasy.Magic;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 장착 마법 무기의 마법·쿨타임·데미지(마법서 배율) 관리
    [RequireComponent(typeof(PlayerLoadout))]
    [DisallowMultipleComponent]
    public sealed class PlayerMagicCaster : MonoBehaviour
    {
        private const float NoMultiplier = 1f;

        private readonly Dictionary<SpellData, float> readyTimes = new Dictionary<SpellData, float>();
        private PlayerLoadout loadout;

        public MagicWeaponData CurrentWeapon => loadout.CurrentWeapon as MagicWeaponData;
        public SpellData CurrentSpell => CurrentWeapon != null ? CurrentWeapon.Spell : null;
        public bool IsEquipped => CurrentSpell != null;
        public bool RequiresTargeting => IsEquipped && CurrentSpell.RequiresTargeting;
        public bool IsReady => IsEquipped && (!readyTimes.TryGetValue(CurrentSpell, out float readyTime) || Time.time >= readyTime);

        private void Awake()
        {
            loadout = GetComponent<PlayerLoadout>();
        }

        public void StartCooldown()
        {
            if (IsEquipped) readyTimes[CurrentSpell] = Time.time + CurrentSpell.Cooldown;
        }

        // 무기 공격력 × 마법 배율 × 마법서 배율
        public int CalculateDamage()
        {
            MagicWeaponData weapon = CurrentWeapon;
            float spellbookMultiplier = loadout.CurrentOffHand is SpellbookData spellbook && spellbook.CanEquipWith(weapon)
                ? spellbook.MagicPowerMultiplier
                : NoMultiplier;

            return Mathf.RoundToInt(weapon.AttackPower * weapon.Spell.DamageMultiplier * spellbookMultiplier);
        }
    }
}
