using System;
using ProjectFantasy.Enemy;
using ProjectFantasy.World;
using UnityEngine;
namespace ProjectFantasy.Objective
{
    // 처치·도착 알림으로 현재 목표 진행도를 세고 단계 완료·변경을 알림
    public sealed class ObjectiveTracker : MonoBehaviour
    {
        [SerializeField] private ObjectiveSequence goals;
        private int progressCount = 0;
        private bool isCompleted = false;
        private int goalIndex = 0;
        public event Action Changed;
        public event Action<ObjectiveData> StepCompleted;
        public bool IsCompleted => isCompleted;
        public ObjectiveData CurrentObjective => isCompleted ? null : goals.Objectives[goalIndex];
        public int ProgressCount => progressCount;
        private void OnEnable()
        {
            EnemyEvents.EnemyKilled += HandleEnemyKilled;
            LocationEvents.Reached += HandleLocationReached;
        }
        private void OnDisable()
        {
            EnemyEvents.EnemyKilled -= HandleEnemyKilled;
            LocationEvents.Reached -= HandleLocationReached;
        }
        private void AddProgress(ObjectiveData currentObjectData)
        {
            bool isStepCompleted = false;
            progressCount++;

            if (progressCount >= currentObjectData.RequiredCount)
            {
                goalIndex++;
                progressCount = 0;
                isStepCompleted = true;
            }

            if (goalIndex >= goals.Objectives.Count)
            {
                isCompleted = true;
            }

            if (isStepCompleted) StepCompleted?.Invoke(currentObjectData);
            Changed?.Invoke();
        }
        private void HandleEnemyKilled(EnemyData enemyData)
        {
            if (isCompleted) return;

            ObjectiveData currentObjectData = goals.Objectives[goalIndex];
            if (!currentObjectData.IsMatch(enemyData)) return;

            AddProgress(currentObjectData);
        }
        private void HandleLocationReached(LocationData locationData)
        {
            if (isCompleted) return;

            ObjectiveData currentObjectData = goals.Objectives[goalIndex];
            if (!currentObjectData.IsMatch(locationData)) return;

            AddProgress(currentObjectData);
        }
    }
}
