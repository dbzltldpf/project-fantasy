using System.Collections.Generic;
using ProjectFantasy.Magic;
using ProjectFantasy.Mastery;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 장착 마법 무기의 마법·쿨타임·데미지(마법서 배율) 관리
    [RequireComponent(typeof(PlayerLoadout), typeof(WeaponMastery))]
    [DisallowMultipleComponent]
    public sealed class PlayerMagicCaster : MonoBehaviour
    {
        private const float NoMultiplier = 1f;

        private readonly Dictionary<SpellData, float> readyTimes = new Dictionary<SpellData, float>();
        private PlayerLoadout loadout;
        private WeaponMastery mastery;

        public MagicWeaponData CurrentWeapon => loadout.CurrentWeapon as MagicWeaponData;
        public SpellData CurrentSpell => CurrentWeapon != null ? CurrentWeapon.Spell : null;
        public bool IsEquipped => CurrentSpell != null;
        public bool RequiresTargeting => IsEquipped && CurrentSpell.RequiresTargeting;
        public bool IsReady => IsEquipped && (!readyTimes.TryGetValue(CurrentSpell, out float readyTime) || Time.time >= readyTime);

        private void Awake()
        {
            loadout = GetComponent<PlayerLoadout>();
            mastery = GetComponent<WeaponMastery>();
        }

        public void StartCooldown()
        {
            if (IsEquipped) readyTimes[CurrentSpell] = Time.time + CurrentSpell.Cooldown;
        }

        // 무기 마법력(숙련 보너스 포함) × 마법 배율 × 활성 마법서 배율(등급) × 마법서 숙련 보너스
        public int CalculateDamage()
        {
            float spellbookMultiplier = loadout.ActiveOffHand is SpellbookData spellbook
                ? spellbook.GetMultiplier(loadout.OffHandInstance) * mastery.GetBonusMultiplier(spellbook)
                : NoMultiplier;
            return Mathf.RoundToInt(loadout.MagicPower * CurrentSpell.DamageMultiplier * spellbookMultiplier);
        }
    }
}
