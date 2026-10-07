using UnityEngine;

namespace ProjectFantasy.UI
{
    // 3조각(둥근 끝) 채움 바 너비 조절 (숙련 경험치: 양 끝 유지 / 체력: 마스크로 잘림)
    public static class BarFill
    {
        private const float EmptyRatio = 0f;

        // fill은 왼쪽 기준 앵커, 부모 너비가 바 전체
        public static void SetRatio(RectTransform fill, float ratio, float minWidth)
        {
            bool isVisible = ratio > EmptyRatio;
            fill.gameObject.SetActive(isVisible);
            if (!isVisible) return;

            float fullWidth = ((RectTransform)fill.parent).rect.width;
            float width = Mathf.Max(minWidth, fullWidth * Mathf.Clamp01(ratio));
            fill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        // 마스크 방식: 채움은 전체 길이 그대로, 마스크(RectMask2D) 너비만 조절 → 줄어든 끝은 직선으로 잘리고 둥근 끝은 제자리
        public static void SetMaskedRatio(RectTransform mask, float ratio)
        {
            float fullWidth = ((RectTransform)mask.parent).rect.width;
            mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullWidth * Mathf.Clamp01(ratio));
        }
    }
}
