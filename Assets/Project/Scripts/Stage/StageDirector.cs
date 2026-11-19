using System;
using System.Collections;
using System.Collections.Generic;
using ProjectFantasy.CameraSystem;
using ProjectFantasy.Combat;
using ProjectFantasy.Enemy;
using ProjectFantasy.Narration;
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
            public NarrationData intro; // 스테이지 시작 시 내레이션 (비우면 없음)
        }
        private const int ObjectivesPerStage = 2;
        [SerializeField] private Stage[] stages;
        [SerializeField] private ObjectiveTracker objectiveTracker;
        [SerializeField] private AreaGate areaGate;
        [SerializeField] private ReachObjectiveData gateObjective;
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Health playerHealth;
        [SerializeField] private NarrationPlayer narrationPlayer;
        [Tooltip("쓰러진 뒤 재시작까지 대기 시간 (사망 모션을 보여 줄 시간)")]
        [SerializeField, Min(0f)] private float retryDelay = 2f;
        private int stageIndex;
        private WaitForSeconds retryWait;
        private Coroutine retryCoroutine;
        private List<ObjectiveData> objectives;
        private List<ObjectiveData> BuildObjectives()
        {
            List<ObjectiveData> objectiveDatas = new List<ObjectiveData>(stages.Length * ObjectivesPerStage);
            for (int i = 0; i < stages.Length; i++)
            {
                objectiveDatas.Add(stages[i].clearObjective);
                objectiveDatas.Add(gateObjective);
            }
            return objectiveDatas;
        }
        private bool IsLastStage => stageIndex == stages.Length - 1;

        public int CurrentStageIndex => stageIndex;
        public int StageCount => stages.Length;
        public GameObject CurrentGroup => stages[stageIndex].group;
        // 마지막 스테이지 클리어 후 게이트 진입 시 알림 (엔딩 진입)
        public event Action AllStagesCleared;

        private void Awake()
        {
            retryWait = new WaitForSeconds(retryDelay);
        }
        // 목표 목록만 준비하고 모든 그룹은 꺼 둠 (시작은 타이틀에서 GoToStage)
        private void Start()
        {
            objectives = BuildObjectives();
            for (int i = 0; i < stages.Length; i++)
            {
                stages[i].group.SetActive(false);
            }
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
            if (stages[stageIndex].intro != null) narrationPlayer.Play(stages[stageIndex].intro);
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
            if (stages[stageIndex].clearObjective != objectiveData) return;
            areaGate.Open();
        }
        private void HandleLocationReached(LocationData locationData)
        {
            if (!gateObjective.IsMatch(locationData) || !areaGate.IsOpen) return;
            if (IsLastStage)
            {
                areaGate.Close();
                AllStagesCleared?.Invoke();
                return;
            }
            MoveToNextStage();
        }
        private void MoveToNextStage()
        {
            GoToStage(stageIndex + 1);
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
