using System;
namespace ProjectFantasy.Combat
{
    // 구간 동안 회피·이동 입력으로 액션 후딜레이를 끊을 수 있음
    [Serializable]
    public sealed class CancelWindowEvent : ActionEvent
    {
        public override void OnBegin(IActionContext context) => context.OpenCancelWindow();
        public override void OnEnd(IActionContext context) => context.CloseCancelWindow();
    }

}
