using System;
using System.Collections.Generic;
using ProjectFantasy.Combat;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProjectFantasy.Enemy
{
    // 근접: 공격 거리까지 접근, 무기 콤보 공격, 방패가 있으면 확률적으로 막기 (값은 EnemyData의 Melee 항목)
    [Serializable]
    public sealed class MeleeCombatStyle : EnemyCombatStyle
    {
        public override float GetAttackRange(EnemyData data) => data.AttackRange;

        public override void TryStartAttack(EnemyController controller)
        {
            EnemyData data = controller.Data;
            if (controller.CanGuard && Random.value < data.Guard.GuardChance)
            {
                controller.ChangeState(controller.GuardState);
                return;
            }

            bool hasCombo = data.ComboData != null && data.ComboData.ActionCount > 0;
            if (hasCombo && controller.IsAttackReady && controller.IsFacingTarget()) controller.ChangeState(controller.AttackState);
        }

        public override void CollectAnimationStates(EnemyData data, ICollection<string> states, ICollection<ActionData> actions)
        {
            if (data.ComboData != null)
            {
                for (int i = 0; i < data.ComboData.ActionCount; i++)
                {
                    actions.Add(data.ComboData.GetAction(i));
                }
            }
            if (data.HasShield && data.Guard.CounterAction != null) actions.Add(data.Guard.CounterAction);
        }

        public override string Validate(EnemyController controller)
        {
            return controller.Data.ComboData == null ? "근접 방식인데 무기에 콤보(Combo Data)가 없습니다." : null;
        }
    }
}
