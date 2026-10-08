using System.Collections.Generic;
using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using ProjectFantasy.Magic;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 시전: 준비된 마법을 가중치로 골라 시전 모션 → 발동 시점에 마법탄(대상 조준) 또는 범위 마법(대상 위치에 마법진 예고) → 복귀
    // 마법별 쿨타임은 적 개체마다 따로 (스타일 데이터는 공유되므로 상태가 보관)
    public sealed class EnemyCastState : EnemyStateBase
    {
        private readonly Dictionary<SpellData, float> readyTimes = new Dictionary<SpellData, float>();
        private CasterCombatStyle style;
        private SpellData spell;
        private float elapsedTime;
        private bool hasReleased;

        public EnemyCastState(EnemyController controller) : base(controller) { }

        // 준비된 마법이 있으면 선택 후 시전 상태로 전이
        public bool TryBegin(CasterCombatStyle casterStyle)
        {
            SpellData chosen = ChooseReadySpell(casterStyle);
            if (chosen == null) return false;

            style = casterStyle;
            spell = chosen;
            Controller.ChangeState(this);
            return true;
        }

        // 리스폰 시 쿨타임 초기화
        public void ResetCooldowns() => readyTimes.Clear();

        public override void Enter()
        {
            elapsedTime = 0f;
            hasReleased = false;
            readyTimes[spell] = Time.time + spell.Cooldown;

            Navigator.Stop();
            EnemyAnimator.PlayState(spell.CastStateHash, true);
        }

        public override void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;

            if (!hasReleased)
            {
                FaceTarget(deltaTime);
                if (elapsedTime >= spell.ReleaseTime) Release();
            }

            if (elapsedTime < spell.CastDuration) return;

            Controller.StartAttackCooldown();
            Controller.ReturnToCombat();
        }

        private void Release()
        {
            hasReleased = true;
            if (!Perception.HasTarget) return;

            int damage = Mathf.RoundToInt(Data.AttackPower * spell.DamageMultiplier);
            Transform target = Perception.Target;

            switch (spell)
            {
                case ProjectileSpellData projectileSpell:
                    Vector3 origin = Controller.transform.TransformPoint(style.CastOffset);
                    ProjectileProfile profile = projectileSpell.ToProfile();
                    Vector3 velocity = (style.GetAimPoint(target) - origin).normalized * profile.Speed;
                    Controller.RangedAttacker.Fire(origin, style.ApplySpread(velocity), profile, damage, DamageType.Magic);
                    break;

                // 발동 시점의 대상 발밑에 고정 → 마법진을 보고 피할 수 있음
                case AreaSpellData areaSpell:
                    Controller.SpellCaster.CastArea(areaSpell, target.position, damage);
                    break;
            }
        }

        // 쿨타임이 끝난 마법끼리 가중치 추첨 (할당 없이 두 번 순회)
        private SpellData ChooseReadySpell(CasterCombatStyle casterStyle)
        {
            float totalWeight = 0f;
            for (int i = 0; i < casterStyle.SpellCount; i++)
            {
                CasterSpell entry = casterStyle.GetSpell(i);
                if (IsReady(entry)) totalWeight += entry.Weight;
            }
            if (totalWeight <= 0f) return null;

            float pick = Random.value * totalWeight;
            SpellData last = null;
            for (int i = 0; i < casterStyle.SpellCount; i++)
            {
                CasterSpell entry = casterStyle.GetSpell(i);
                if (!IsReady(entry)) continue;

                last = entry.Spell;
                pick -= entry.Weight;
                if (pick <= 0f) return entry.Spell;
            }
            return last;
        }

        private bool IsReady(CasterSpell entry)
        {
            if (entry.Spell == null || entry.Weight <= 0f) return false;
            return !readyTimes.TryGetValue(entry.Spell, out float readyTime) || Time.time >= readyTime;
        }
    }
}
