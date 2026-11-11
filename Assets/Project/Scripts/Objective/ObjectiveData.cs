using ProjectFantasy.Enemy;
using UnityEngine;
namespace ProjectFantasy.Objective
{
    // 목표 한 단계: 화면 문구, 대상, 필요 횟수
    [CreateAssetMenu(fileName = "ObjectiveData", menuName = "ProjectFantasy/Objective/Objective Data")]
    public sealed class ObjectiveData : ScriptableObject
    {
        [SerializeField] private string description;
        [SerializeField] private EnemyData target;
        [SerializeField, Min(1)] private int requiredCount;

        public string Description => description;
        public EnemyData Target => target;
        public int RequiredCount => requiredCount;
    }
}

