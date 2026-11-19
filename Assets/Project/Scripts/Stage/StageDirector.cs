using System;
using System.Collections;
using System.Collections.Generic;
using ProjectFantasy.CameraSystem;
using ProjectFantasy.Combat;
using ProjectFantasy.Enemy;
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
        private const int ObjectivesPerStage = 2;
        [SerializeField] private Stage[] stages;
        [SerializeField] private ObjectiveTracker objectiveTracker;
        [SerializeField] private AreaGate areaGate;
        [SerializeField] private ReachObjectiveData gateObjective;
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private PlayerMotor playerMotor;
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Health playerHealth;
        [Tooltip("쓰러진 뒤 재시작까지 대기 시간 (사망 모션을 보여 줄 시간)")]
        [SerializeField, Min(0f)] private float retryDelay = 2f;
        private int stageIndex;
        private bool HasCurrentStage => stageIndex < stages.Length;
        private WaitForSeconds retryWait;
        private Coroutine retryCoroutine;
        private List<ObjectiveData> objectives;
        private List<ObjectiveData> BuildObjectives()
        {
            List<ObjectiveData> objectiveDatas = new List<ObjectiveData>(stages.Length * ObjectivesPerStage);
            for (int i = 0; i < stages.Length; i++)
            {
                objectiveDatas.Add(stages[i].clearObjective);
                if (i != stages.Length - 1) objectiveDatas.Add(gateObjective);
            }
            return objectiveDatas;
        }
        private bool IsLastStage => stageIndex == stages.Length - 1;

        public int CurrentStageIndex => stageIndex;
        public int StageCount => stages.Length;
        public GameObject CurrentGroup => stages[stageIndex].group;
        private void Awake()
        {
            retryWait = new WaitForSeconds(retryDelay);
        }
        private void Start()
        {
            objectives = BuildObjectives();
            GoToStage(0);
        }
        public void GoToStage(int index)
        {
            if (index < 0 || index >= stages.Length) return;
            stageIndex = index;

            for (int i = 0; i < stages.Length; i++)
            {
                stages[i].group.SetActive(i == stageIndex);
            }
            ResetCurrentStageEnemies();
            areaGate.Close();

            objectiveTracker.Begin(objectives, stageIndex * ObjectivesPerStage);
            playerController.Revive(playerSpawn.position, playerSpawn.rotation);
            thirdPersonCamera.SnapToTarget();
        }
        private void OnEnable()
        {
            objectiveTracker.StepCompleted += HandleStepCompleted;
            LocationEvents.Reached += HandleLocationReached;
            playerHealth.Died += HandlePlayerDied;
        }
        private void OnDisable()
        {
            objectiveTracker.StepCompleted -= HandleStepCompleted;
            LocationEvents.Reached -= HandleLocationReached;
            playerHealth.Died -= HandlePlayerDied;

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
        private void HandlePlayerDied()
        {
            if (retryCoroutine != null) return;
            retryCoroutine = StartCoroutine(RetryAfterDelay());
        }
        private IEnumerator RetryAfterDelay()
        {
            yield return retryWait;
            Retry();
            retryCoroutine = null;
        }
        private void ResetCurrentStageEnemies()
        {
            EnemySpawner[] spawners = stages[stageIndex].group.GetComponentsInChildren<EnemySpawner>();
            for (int i = 0; i < spawners.Length; i++)
            {
                spawners[i].ResetEnemies();
            }
        }
        private void Retry()
        {
            if (!areaGate.IsOpen)
            {
                ResetCurrentStageEnemies();
                objectiveTracker.RestartStep();
            }
            playerController.Revive(playerSpawn.position, playerSpawn.rotation);
            thirdPersonCamera.SnapToTarget();
        }
    }
}
