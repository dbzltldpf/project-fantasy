using System;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 액션 타임라인 이벤트 베이스: [startFrame, endFrame) 구간 활성 (에셋 공유 데이터이므로 런타임 상태는 컨텍스트에 둘 것)
    [Serializable]
    public abstract class ActionEvent
    {
        [SerializeField, Min(0)] private int startFrame;
        [SerializeField, Min(0)] private int endFrame = 1;

        public int StartFrame => startFrame;
        public int EndFrame => endFrame;

        protected ActionEvent() { }

        protected ActionEvent(int startFrame, int endFrame)
        {
            this.startFrame = startFrame;
            this.endFrame = endFrame;
        }

        public bool Contains(float frame) => frame >= startFrame && frame < endFrame;

        public virtual void OnBegin(IActionContext context) { }
        public virtual void OnTick(IActionContext context) { }
        public virtual void OnEnd(IActionContext context) { }

        // 에디터 검증 (문제 없으면 null)
        public virtual string Validate(int lengthFrames)
        {
            if (startFrame >= endFrame) return "startFrame >= endFrame (구간이 비어 있어 순간 1회만 실행됨)";
            if (endFrame > lengthFrames) return "endFrame > lengthFrames (액션 종료 후 구간)";
            return null;
        }
    }
}
