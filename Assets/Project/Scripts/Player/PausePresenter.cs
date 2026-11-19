using ProjectFantasy.CameraSystem;
using ProjectFantasy.UI;
using UnityEngine;
namespace ProjectFantasy.Player
{
    // ESC로 일시정지·해제 (시간·조작·시점 정지, 메뉴 표시)
    public sealed class PausePresenter : MonoBehaviour
    {
        [Tooltip("ESC 입력 수신, 게임플레이 입력 차단")]
        [SerializeField] private PlayerInputHandler inputHandler;
        [Tooltip("일시정지 중 시점 회전 정지")]
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [Tooltip("일시정지 메뉴 UI")]
        [SerializeField] private PauseMenuView pauseMenuView;
        [Tooltip("열려 있으면 ESC 무시")]
        [SerializeField] private InventoryWindow inventoryWindow;
        [Tooltip("열려 있으면 ESC 무시")]
        [SerializeField] private MasteryWindow masteryWindow;
        public float GameSpeed { get; set; } = 1f;
        public bool IsPaused { get; private set; }

        private void OnEnable()
        {
            inputHandler.PausePressed += HandlePausePressed;
        }
        private void OnDisable()
        {
            inputHandler.PausePressed -= HandlePausePressed;
        }
        private void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            inputHandler.SetGameplayEnabled(false);
            thirdPersonCamera.SetLookEnabled(false);
            pauseMenuView.Show();
        }
        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = GameSpeed;
            inputHandler.SetGameplayEnabled(true);
            thirdPersonCamera.SetLookEnabled(true);
            pauseMenuView.Hide();
        }
        private void HandlePausePressed()
        {
            if (inventoryWindow.IsOpen || masteryWindow.IsOpen) return;
            if (IsPaused) Resume(); else Pause();
        }
    }

}
