using UnityEngine;
namespace ProjectFantasy.Player
{
    // 활 당김 비율 (조준 중 누적, 발사 시 속도·데미지 배율 결정)
    public sealed class RangedDraw
    {
        public float Ratio { get; private set; }
        public bool IsDrawing { get; private set; }
        private float duration;
        private float elapsed;
        public float Elapsed => elapsed;
        public void Begin(float drawDuration)
        {
            duration = drawDuration;
            elapsed = 0;
            IsDrawing = drawDuration > 0f;
            Ratio = IsDrawing ? 0 : 1;
        }
        public void Tick(float deltaTime)
        {
            if (!IsDrawing) return;
            elapsed += deltaTime;
            Ratio = Mathf.Clamp01(elapsed / duration);
        }
        public void Reset()
        {
            IsDrawing = false;
            Ratio = 0;
        }
    }

}
