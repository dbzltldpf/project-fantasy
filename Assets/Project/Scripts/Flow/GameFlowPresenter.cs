using ProjectFantasy.CameraSystem;
using ProjectFantasy.Player;
using ProjectFantasy.Stage;
using ProjectFantasy.UI;
using UnityEngine;
namespace ProjectFantasy.Flow
{
    // 타이틀 → 플레이 → 엔딩 화면 전환, 화면에 따라 플레이어 입력·시점 켜고 끔
    public sealed class GameFlowPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [SerializeField] private StageDirector stageDirector;
        [SerializeField] private TitleView titleView;
        [SerializeField] private PlayRecord playRecord;
        [SerializeField] private EndingView endingView;

        private void Start()
        {
            ShowTitle();
        }
        private void ShowTitle()
        {
            endingView.Hide();
            SetInputEnabled(false);
            titleView.Show();
        }
        private void OnEnable()
        {
            titleView.StartClicked += StartGame;
            titleView.QuitClicked += QuitGame;
            stageDirector.AllStagesCleared += HandleAllStagesCleared;
            endingView.TitleClicked += ShowTitle;
        }
        private void OnDisable()
        {
            titleView.StartClicked -= StartGame;
            titleView.QuitClicked -= QuitGame;
            stageDirector.AllStagesCleared -= HandleAllStagesCleared;
            endingView.TitleClicked -= ShowTitle;
        }
        private void StartGame()
        {
            titleView.Hide();
            playRecord.Begin();
            stageDirector.GoToStage(0);
            SetInputEnabled(true);
        }
        private void QuitGame()
        {
            Application.Quit();
        }
        private void HandleAllStagesCleared()
        {
            playRecord.End();
            SetInputEnabled(false);
            endingView.Show(playRecord.PlayTime, playRecord.DeathCount);
        }
        // 입력 핸들러를 꺼서 ESC·Tab·K까지 막음 (일시정지·메뉴 쪽 수정 없이)
        private void SetInputEnabled(bool isEnabled)
        {
            inputHandler.enabled = isEnabled;
            inputHandler.SetGameplayEnabled(isEnabled);
            thirdPersonCamera.SetLookEnabled(isEnabled);
        }
    }

}
