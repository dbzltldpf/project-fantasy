using ProjectFantasy.Enemy;
using UnityEngine;
namespace ProjectFantasy.Objective
{
    // 적 처치 목표: 대상 적을 필요 수만큼 처치
    [CreateAssetMenu(fileName = "KillObjectiveData", menuName = "ProjectFantasy/Objective/Kill Objective")]
    public sealed class KillObjectiveData : ObjectiveData
    {
        [SerializeField] private EnemyData target;
        [SerializeField, Min(1)] private int requiredCount;
        public override bool IsMatch(EnemyData enemyData) => enemyData == target;
        public override int RequiredCount => requiredCount;

    }
}

