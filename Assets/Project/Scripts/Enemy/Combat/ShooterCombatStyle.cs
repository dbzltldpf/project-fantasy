using System;
using System.Collections.Generic;
using ProjectFantasy.Combat;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 사격(활·석궁): (석궁) 장전 → 조준(클립 끝까지 + 추가 유지) → 발사, 모션 이름·발사 시점·발사 위치·화살은 RangedWeaponData 재사용, 탄약 무한
    [Serializable]
    public sealed class ShooterCombatStyle : EnemyRangedStyle
    {
        [Tooltip("조준 클립이 끝난 뒤 조준 자세를 더 유지하는 시간 (초, 플레이어가 피할 여유)")]
        [SerializeField, Min(0f)] private float aimDuration = 0.2f;

        public float AimDuration => aimDuration;

        public override void TryStartAttack(EnemyController controller)
        {
            if (controller.IsAttackReady && controller.IsFacingTarget()) controller.ChangeState(controller.ShootState);
        }

        public override void CollectAnimationStates(EnemyData data, ICollection<string> states, ICollection<ActionData> actions)
        {
            if (!(data.Weapon is RangedWeaponData weapon)) return;

            states.Add(weapon.AimIdleStateName);
            states.Add(weapon.FireStateName);
            if (weapon.RequiresReload) states.Add(weapon.ReloadStateName);
        }

        public override void CollectHoldStates(EnemyData data, ICollection<string> states)
        {
            if (data.Weapon is RangedWeaponData weapon) states.Add(weapon.AimIdleStateName);
        }

        public override string Validate(EnemyController controller)
        {
            if (!(controller.Data.Weapon is RangedWeaponData weapon) || weapon.Ammo == null) return "사격 방식에는 화살(Ammo)이 지정된 Ranged Weapon Data가 필요합니다.";
            return controller.RangedAttacker == null ? "사격 방식에는 RangedAttacker 컴포넌트가 필요합니다." : null;
        }
    }
}
