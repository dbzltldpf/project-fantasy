using System;
using System.Collections.Generic;
using ProjectFantasy.Enemy;
using ProjectFantasy.World;
using UnityEngine;
namespace ProjectFantasy.Objective
{
    // 처치·도착 알림으로 현재 목표 진행도를 세고 단계 완료·변경을 알림
    public sealed class ObjectiveTracker : MonoBehaviour
    {
        private IReadOnlyList<ObjectiveData> objectives;
        private int progressCount = 0;
        private bool isCompleted = false;
        private int goalIndex = 0;
        public event Action Changed;
        public event Action<ObjectiveData> StepCompleted;
        public bool IsCompleted => isCompleted;
        public ObjectiveData CurrentObjective => objectives == null || isCompleted ? null : objectives[goalIndex];
        public int ProgressCount => progressCount;
        private bool IsRunning => objectives != null && !isCompleted;
        public void Begin(IReadOnlyList<ObjectiveData> objectives)
        {
            this.objectives = objectives;
            goalIndex = 0;
            progressCount = 0;
            isCompleted = objectives.Count == 0;
            Changed?.Invoke();
        }
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

            if (goalIndex >= objectives.Count)
            {
                isCompleted = true;
            }

            if (isStepCompleted) StepCompleted?.Invoke(currentObjectData);
            Changed?.Invoke();
        }
        // 현재 단계 진행 수만 초기화 (스테이지 재시작)
        public void RestartStep()
        {
            if (!IsRunning) return;
            progressCount = 0;
            Changed?.Invoke();
        }
        private void HandleEnemyKilled(EnemyData enemyData)
        {
            if (!IsRunning) return;

            ObjectiveData currentObjectData = objectives[goalIndex];
            if (!currentObjectData.IsMatch(enemyData)) return;

            AddProgress(currentObjectData);
        }
        private void HandleLocationReached(LocationData locationData)
        {
            if (!IsRunning) return;

            ObjectiveData currentObjectData = objectives[goalIndex];
            if (!currentObjectData.IsMatch(locationData)) return;

            AddProgress(currentObjectData);
        }
    }
}
