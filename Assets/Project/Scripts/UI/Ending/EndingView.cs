using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace ProjectFantasy.UI
{
    // 엔딩 화면 표시(플레이 시간·사망 횟수), 타이틀로 버튼 클릭 알림
    [DisallowMultipleComponent]
    public sealed class EndingView : MonoBehaviour
    {
        private const int SecondsPerMinute = 60;
        private const string PlayTimeFormat = "플레이 시간 {0:00}:{1:00}";
        private const string DeathCountFormat = "쓰러진 횟수 {0}회";
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text playTimeLabel;
        [SerializeField] private TMP_Text deathCountLabel;
        [SerializeField] private Button titleButton;
        public event Action TitleClicked;

        private void Awake()
        {
            root.SetActive(false);
            titleButton.onClick.AddListener(() => TitleClicked?.Invoke());
        }
        public void Show(float playTime, int deathCount)
        {
            int totalSeconds = Mathf.FloorToInt(playTime);
            playTimeLabel.text = string.Format(PlayTimeFormat, totalSeconds / SecondsPerMinute, totalSeconds % SecondsPerMinute);
            deathCountLabel.text = string.Format(DeathCountFormat, deathCount);
            root.SetActive(true);
        }
        public void Hide()
        {
            root.SetActive(false);
        }
    }
}
