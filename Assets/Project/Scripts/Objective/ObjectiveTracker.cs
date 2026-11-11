using System;
using ProjectFantasy.Enemy;
using UnityEngine;
namespace ProjectFantasy.Objective
{
    // 적 처치 알림으로 현재 목표 진행도를 세고 단계 완료·변경을 알림
    public sealed class ObjectiveTracker : MonoBehaviour
    {
        [SerializeField] private ObjectiveSequence goals;
        private int killCount = 0;
        private bool isCompleted = false;
        private int goalIndex = 0;
        public event Action Changed;
        public event Action<ObjectiveData> StepCompleted;
        public bool IsCompleted => isCompleted;
        public ObjectiveData CurrentObjective => isCompleted ? null : goals.Objectives[goalIndex];
        public int KillCount => killCount;
        private void OnEnable()
        {
            EnemyEvents.EnemyKilled += HandleEnemyKilled;
        }
        private void OnDisable()
        {
            EnemyEvents.EnemyKilled -= HandleEnemyKilled;
        }
        private void HandleEnemyKilled(EnemyData enemyData)
        {
            bool isStepCompleted = false;

            if (isCompleted) return;

            ObjectiveData currentObjectData = goals.Objectives[goalIndex];
            if (enemyData != currentObjectData.Target) return;

            killCount++;

            if (killCount >= currentObjectData.RequiredCount)
            {
                goalIndex++;
                killCount = 0;
                isStepCompleted = true;
            }

            if (goalIndex >= goals.Objectives.Count)
            {
                isCompleted = true;
            }

            if (isStepCompleted) StepCompleted?.Invoke(currentObjectData);
            Changed?.Invoke();
        }
    }
}
