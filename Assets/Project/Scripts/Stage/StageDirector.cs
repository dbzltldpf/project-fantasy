using System;
using System.Collections.Generic;
using ProjectFantasy.CameraSystem;
using ProjectFantasy.Objective;
using ProjectFantasy.Player;
using ProjectFantasy.World;
using UnityEngine;
namespace ProjectFantasy.Stage
{
    // 스테이지 순서 진행: 클리어 시 게이트 개방, 게이트 진입 시 이동·다음 적 그룹 활성화
    public sealed class StageDirector : MonoBehaviour
    {
        [Serializable]
        private struct Stage
        {
            public GameObject group; // 이 스테이지의 적 묶음 (스포너 + 구역)
            public ObjectiveData clearObjective; // 끝나면 게이트를 열 목표
        }

        [SerializeField] private Stage[] stages;
        [SerializeField] private ObjectiveTracker objectiveTracker;
        [SerializeField] private AreaGate areaGate;
        [SerializeField] private ReachObjectiveData gateObjective;
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private PlayerMotor playerMotor;
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        private int stageIndex;
        private bool HasCurrentStage => stageIndex < stages.Length;
        private List<ObjectiveData> BuildObjectives()
        {
            List<ObjectiveData> objectiveDatas = new List<ObjectiveData>(stages.Length * 2);
            for (int i = 0; i < stages.Length; i++)
            {
                objectiveDatas.Add(stages[i].clearObjective);
                if (i != stages.Length - 1) objectiveDatas.Add(gateObjective);
            }
            return objectiveDatas;
        }
        private bool IsLastStage => stageIndex == stages.Length - 1;
        private void Start()
        {
            objectiveTracker.Begin(BuildObjectives());
            for (int i = 0; i < stages.Length; i++)
            {
                stages[i].group.SetActive(i == stageIndex);
            }
        }
        private void OnEnable()
        {
            objectiveTracker.StepCompleted += HandleStepCompleted;
            LocationEvents.Reached += HandleLocationReached;
        }
        private void OnDisable()
        {
            objectiveTracker.StepCompleted -= HandleStepCompleted;
            LocationEvents.Reached -= HandleLocationReached;
        }
        private void HandleStepCompleted(ObjectiveData objectiveData)
        {
            if (!HasCurrentStage) return;
            if (stages[stageIndex].clearObjective != objectiveData) return;
            if (IsLastStage) return;
            areaGate.Open();
        }
        private void HandleLocationReached(LocationData locationData)
        {
            if (!HasCurrentStage) return;
            if (!gateObjective.IsMatch(locationData) || !areaGate.IsOpen) return;
            MoveToNextStage();
        }
        private void MoveToNextStage()
        {
            playerMotor.Teleport(playerSpawn.position, playerSpawn.rotation);
            thirdPersonCamera.SnapToTarget();
            areaGate.Close();
            stages[stageIndex].group.SetActive(false);
            stageIndex++;
            if (stageIndex >= stages.Length) return;
            stages[stageIndex].group.SetActive(true);
        }
    }
}
