using System;
using System.Collections.Generic;
using ProjectFantasy.Combat;
using ProjectFantasy.Magic;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 시전(마법): 준비된 마법 중 가중치로 하나 선택 → 시전 모션 → 마법탄 발사 또는 대상 위치 범위 마법(마법진 예고 후 발동)
    [Serializable]
    public sealed class CasterCombatStyle : EnemyRangedStyle
    {
        [Tooltip("마법탄 발사 위치 (적 로컬, 지팡이 끝 근처)")]
        [SerializeField] private Vector3 castOffset = new Vector3(0f, 1.5f, 0.6f);
        [Tooltip("사용할 마법 (쿨타임·시전 모션·피해 배율은 각 SpellData 값)")]
        [SerializeField] private CasterSpell[] spells = Array.Empty<CasterSpell>();

        public Vector3 CastOffset => castOffset;
        public int SpellCount => spells.Length;
        public CasterSpell GetSpell(int index) => spells[index];

        public override void TryStartAttack(EnemyController controller)
        {
            if (controller.IsAttackReady && controller.IsFacingTarget()) controller.CastState.TryBegin(this);
        }

        public override void CollectAnimationStates(EnemyData data, ICollection<string> states, ICollection<ActionData> actions)
        {
            foreach (CasterSpell entry in spells)
            {
                if (entry.Spell != null) states.Add(entry.Spell.CastStateName);
            }
        }

        public override string Validate(EnemyController controller)
        {
            foreach (CasterSpell entry in spells)
            {
                if (entry.Spell is ProjectileSpellData && controller.RangedAttacker == null) return "마법탄에는 RangedAttacker 컴포넌트가 필요합니다.";
                if (entry.Spell is AreaSpellData && controller.SpellCaster == null) return "범위 마법에는 SpellCaster 컴포넌트가 필요합니다.";
            }
            return spells.Length == 0 ? "시전 방식에 마법(Spells)이 없습니다." : null;
        }
    }
}
