using System;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 구간 동안 다음 공격 입력 예약, 예약되면 transitionFrame에 다음 액션으로 전이
    [Serializable]
    public sealed class ComboWindowEvent : ActionEvent
    {
        [Tooltip("예약된 입력이 있으면 이 프레임부터 다음 액션으로 전이")]
        [SerializeField, Min(0)] private int transitionFrame;

        public ComboWindowEvent() { }

        public ComboWindowEvent(int startFrame, int endFrame, int transitionFrame) : base(startFrame, endFrame)
        {
            this.transitionFrame = transitionFrame;
        }

        public override void OnBegin(IActionContext context) => context.OpenComboWindow(transitionFrame);
        public override void OnEnd(IActionContext context) => context.CloseComboWindow();

        public override string Validate(int lengthFrames)
        {
            if (transitionFrame < StartFrame) return "transitionFrame < startFrame (입력 전에 전이 시점이 지남)";
            if (transitionFrame > lengthFrames) return "transitionFrame > lengthFrames (콤보 연계 불가)";
            return base.Validate(lengthFrames);
        }
    }
}
