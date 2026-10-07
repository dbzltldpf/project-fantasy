using TMPro;
using UnityEngine;

namespace ProjectFantasy.UI
{
    // 숙련도 창 한 줄: 이름, 레벨, 해금 티어, 경험치 바(양 끝 둥근 조각 유지), 보너스
    [DisallowMultipleComponent]
    public sealed class MasteryRowView : MonoBehaviour
    {
        private const float PercentScale = 100f;
        private const float NoBonus = 1f;
        private const float FullRatio = 1f;
        private const float EmptyRatio = 0f;
        private const int NoRequirement = 0;

        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private TMP_Text tierLabel;
        [Tooltip("채움 영역 (왼쪽 기준, 너비 = 바 전체 × 경험치 비율, 자식에 Left/Mid/Right 조각)")]
        [SerializeField] private RectTransform experienceFill;
        [Tooltip("양 끝 조각 너비 합 (이보다 좁아지면 둥근 끝이 찌그러짐)")]
        [SerializeField, Min(0f)] private float minFillWidth = 18f;
        [SerializeField] private TMP_Text experienceLabel;
        [SerializeField] private TMP_Text bonusLabel;
        [Tooltip("최대 레벨일 때 경험치 문구")]
        [SerializeField] private string maxLevelText = "MAX";

        public void SetName(string masteryName) => nameLabel.text = masteryName;

        // required가 0이면 최대 레벨
        public void SetProgress(int level, int tier, int experience, int required, float bonusMultiplier)
        {
            levelLabel.SetText("Lv {0}", level);
            tierLabel.SetText("T{0}", tier);

            bool isMaxLevel = required <= NoRequirement;
            SetFillRatio(isMaxLevel ? FullRatio : (float)experience / required);
            if (isMaxLevel) experienceLabel.text = maxLevelText;
            else experienceLabel.SetText("{0} / {1}", experience, required);

            bonusLabel.SetText("+{0:1}%", (bonusMultiplier - NoBonus) * PercentScale);
        }

        // 0이면 숨김, 그 외에는 양 끝 조각 너비 이상 유지
        private void SetFillRatio(float ratio)
        {
            bool isVisible = ratio > EmptyRatio;
            experienceFill.gameObject.SetActive(isVisible);
            if (!isVisible) return;

            float fullWidth = ((RectTransform)experienceFill.parent).rect.width;
            float width = Mathf.Max(minFillWidth, fullWidth * Mathf.Clamp01(ratio));
            experienceFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }
    }
}
