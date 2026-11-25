using System;
using ProjectFantasy.Dev;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace ProjectFantasy.UI
{
    // 테스트 버튼 클릭을 명령으로 알림 (게임 시스템은 모름)
    public sealed class DevPanelView : MonoBehaviour
    {
        [Tooltip("다음 스테이지 버튼")]
        [SerializeField] private Button previousStage;
        [Tooltip("다음 스테이지 버튼")]
        [SerializeField] private Button nextStage;
        [Tooltip("플레이어 무적 토글 버튼")]
        [SerializeField] private Button toggleInvincible;
        [Tooltip("현재 스테이지 적 전멸 버튼")]
        [SerializeField] private Button killStageEnemies;
        [Tooltip("플레이어 즉사 버튼")]
        [SerializeField] private Button killPlayer;
        [Tooltip("배속 순환 버튼 (x1 → x2 → x0.5)")]
        [SerializeField] private Button cycleGameSpeed;
        [Tooltip("현재 배속 표시 텍스트")]
        [SerializeField] private TMP_Text speedLabel;
        [Tooltip("종료 버튼")]
        [SerializeField] private Button exit;

        public event Action<DevCommand> CommandChosen;
        private void Awake()
        {
            previousStage.onClick.AddListener(() => CommandChosen?.Invoke(DevCommand.PreviousStage));
            nextStage.onClick.AddListener(() => CommandChosen?.Invoke(DevCommand.NextStage));
            toggleInvincible.onClick.AddListener(() => CommandChosen?.Invoke(DevCommand.ToggleInvincible));
            killStageEnemies.onClick.AddListener(() => CommandChosen?.Invoke(DevCommand.KillStageEnemies));
            killPlayer.onClick.AddListener(() => CommandChosen?.Invoke(DevCommand.KillPlayer));
            cycleGameSpeed.onClick.AddListener(() => CommandChosen?.Invoke(DevCommand.CycleGameSpeed));
            exit.onClick.AddListener(() => CommandChosen?.Invoke(DevCommand.Exit));
        }
        public void SetSpeedLabel(float speed)
        {
            speedLabel.text = $"배속 x{speed}";
        }
    }

}
