using System;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 구간 동안 정면으로 전진 (공격 시 내딛기)
    [Serializable]
    public sealed class MoveEvent : ActionEvent
    {
        private const float StopSpeed = 0f;

        [Tooltip("전진 속도 (m/s, 재생 속도 배율 적용)")]
        [SerializeField, Min(0f)] private float speed = 2f;

        public MoveEvent() { }

        public MoveEvent(int startFrame, int endFrame, float speed) : base(startFrame, endFrame)
        {
            this.speed = speed;
        }

        public override void OnBegin(IActionContext context) => context.SetMoveSpeed(speed);
        public override void OnEnd(IActionContext context) => context.SetMoveSpeed(StopSpeed);
    }
}
