using System;
using System.Collections.Generic;
using ProjectFantasy.Combat;

namespace ProjectFantasy.Enemy
{
    // 적 전투 방식 (EnemyData에서 드롭다운으로 선택): 공격 거리, 공격 시작 판단, 필요한 애니메이터 상태
    // 데이터 에셋이 여러 적에 공유되므로 런타임 상태(쿨타임 등)는 두지 않음
    [Serializable]
    public abstract class EnemyCombatStyle
    {
        // 공격 거리 (이보다 멀면 접근)
        public abstract float GetAttackRange(EnemyData data);

        // 사선이 가리면 공격하지 않고 접근
        public virtual bool RequiresLineOfSight => false;

        // 공격 거리 안에서 대상을 바라보는 중 매 프레임 호출 (공격 상태로 전이하면 됨)
        public abstract void TryStartAttack(EnemyController controller);

        // 애니메이터 생성 도구용: 일반 상태 이름 / 액션 타임라인 상태(ActionSpeed 연결)
        public abstract void CollectAnimationStates(EnemyData data, ICollection<string> states, ICollection<ActionData> actions);

        // 애니메이터 생성 도구용: 자세 고정(PoseSpeed)을 연결할 상태 (조준 대기 등)
        public virtual void CollectHoldStates(EnemyData data, ICollection<string> states) { }

        // 필수 컴포넌트·무기 종류 확인, 문제가 있으면 메시지 반환 (없으면 null)
        public virtual string Validate(EnemyController controller) => null;
    }
}
