using ProjectFantasy.Combat;
using ProjectFantasy.Flow;
using ProjectFantasy.Player;
using ProjectFantasy.Stage;
using ProjectFantasy.UI;
using UnityEngine;
namespace ProjectFantasy.Dev
{
    // 테스트 패널 명령 실행 (스테이지 이동·무적·전멸·즉사·배속·타이틀 복귀)
    public sealed class DevCommands : MonoBehaviour
    {
        private const int FirstStageIndex = 0;
        [Tooltip("테스트 버튼 패널")]
        [SerializeField] private DevPanelView devPanelView;
        [Tooltip("스테이지 이동·현재 적 그룹 조회")]
        [SerializeField] private StageDirector stageDirector;
        [Tooltip("명령 후 일시정지 해제, 배속 적용")]
        [SerializeField] private PausePresenter pausePresenter;
        [Tooltip("무적·즉사 대상 플레이어 체력")]
        [SerializeField] private Health playerHealth;
        [Tooltip("종료 시 타이틀 화면 복귀")]
        [SerializeField] private GameFlowPresenter gameFlowPresenter;

        private bool isInvincible;
        private static readonly float[] SpeedSteps = { 1f, 2f, 0.5f };
        private int speedIndex;
        private void OnEnable()
        {
            devPanelView.CommandChosen += HandleCommand;
        }
        private void OnDisable()
        {
            devPanelView.CommandChosen -= HandleCommand;
        }
        private void HandleCommand(DevCommand command)
        {
            switch (command)
            {
                case DevCommand.PreviousStage:
                    GoToPreviousStage();
                    break;
                case DevCommand.NextStage:
                    GoToNextStage();
                    break;
                case DevCommand.ToggleInvincible:
                    ToggleInvincible();
                    break;
                case DevCommand.KillStageEnemies:
                    KillStageEnemies();
                    break;
                case DevCommand.KillPlayer:
                    KillPlayer();
                    break;
                case DevCommand.CycleGameSpeed:
                    CycleGameSpeed();
                    return;
                case DevCommand.Exit:
                    Exit();
                    return;
            }
            pausePresenter.Resume();
        }
        private void GoToPreviousStage()
        {
            if (stageDirector.CurrentStageIndex <= FirstStageIndex) return;
            stageDirector.GoToStage(stageDirector.CurrentStageIndex - 1);
        }

        private void GoToNextStage()
        {
            if (stageDirector.CurrentStageIndex + 1 >= stageDirector.StageCount) return;
            stageDirector.GoToStage(stageDirector.CurrentStageIndex + 1);
        }
        private void ToggleInvincible()
        {
            isInvincible = !isInvincible;
            playerHealth.SetInvincible(isInvincible);
        }
        private void KillStageEnemies()
        {
            foreach (Health enemyHealth in stageDirector.CurrentGroup.GetComponentsInChildren<Health>())
                enemyHealth.Kill();
        }
        private void KillPlayer()
        {
            isInvincible = false;
            playerHealth.Kill();
        }
        private void CycleGameSpeed()
        {
            speedIndex = (speedIndex + 1) % SpeedSteps.Length;
            pausePresenter.GameSpeed = SpeedSteps[speedIndex];
            devPanelView.SetSpeedLabel(pausePresenter.GameSpeed);
        }
        private void Exit()
        {
            gameFlowPresenter.RestartToTitle();
        }

    }

}
