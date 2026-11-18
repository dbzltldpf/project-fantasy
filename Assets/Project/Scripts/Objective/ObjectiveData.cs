using ProjectFantasy.Enemy;
using ProjectFantasy.World;
using UnityEngine;
namespace ProjectFantasy.Objective
{
    // 목표 한 단계의 공통 틀: 화면 문구, 필요 횟수, 알림 일치 여부
    public abstract class ObjectiveData : ScriptableObject
    {
        [SerializeField] private string description;
        public string Description => description;
        public abstract int RequiredCount { get; }
        public virtual bool IsMatch(EnemyData enemyData) => false;
        public virtual bool IsMatch(LocationData locationData) => false;
        public virtual bool ShowProgress => true;
    }

}
