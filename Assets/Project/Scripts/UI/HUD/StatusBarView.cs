using TMPro;
using UnityEngine;

namespace ProjectFantasy.UI
{
    // 자원 바 한 줄 (체력·스태미나·마나 공용): 어두운 배경 + 마스크 채움, 수치, 낮을 때 채움 깜빡임
    [DisallowMultipleComponent]
    public sealed class StatusBarView : MonoBehaviour
    {
        private const float OpaqueAlpha = 1f;
        private const float NoWarning = 0f;

        [Tooltip("채움 마스크 (RectMask2D, 왼쪽 기준, 자식 Fill은 전체 길이 고정)")]
        [SerializeField] private RectTransform fillMask;
        [Tooltip("깜빡일 채움 영역의 CanvasGroup")]
        [SerializeField] private CanvasGroup fillGroup;
        [SerializeField] private TMP_Text valueLabel;
        [Tooltip("{0} = 현재, {1} = 최대")]
        [SerializeField] private string valueFormat = "{0} / {1}";

        [Header("Low Warning")]
        [Tooltip("이 비율 이하면 채움 깜빡임 (0이면 사용 안 함)")]
        [SerializeField, Range(0f, 1f)] private float warningThreshold = 0.3f;
        [Tooltip("깜빡임 속도 (클수록 빠름, 2 = 1초에 한 번)")]
        [SerializeField, Min(0f)] private float blinkSpeed = 2f;
        [Tooltip("깜빡일 때 가장 흐린 알파")]
        [SerializeField, Range(0f, 1f)] private float minBlinkAlpha = 0.35f;

        private bool isWarning;

        // 경고 중일 때만 알파 갱신
        private void Update()
        {
            if (!isWarning) return;

            float pulse = Mathf.PingPong(Time.time * blinkSpeed, OpaqueAlpha);
            fillGroup.alpha = Mathf.Lerp(minBlinkAlpha, OpaqueAlpha, pulse);
        }

        public void SetValue(int current, int max)
        {
            float ratio = max > 0 ? (float)current / max : 0f;
            BarFill.SetMaskedRatio(fillMask, ratio);
            valueLabel.SetText(valueFormat, current, max);
            SetWarning(warningThreshold > NoWarning && current > 0 && ratio <= warningThreshold);
        }

        private void SetWarning(bool warning)
        {
            if (isWarning == warning) return;

            isWarning = warning;
            if (!warning) fillGroup.alpha = OpaqueAlpha;
        }
    }
}
