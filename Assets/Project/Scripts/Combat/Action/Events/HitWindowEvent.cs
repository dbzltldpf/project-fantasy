using System;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 타격 판정 구간 (데미지 = 무기 공격력 × 배율, 구간마다 대상당 1회)
    [Serializable]
    public sealed class HitWindowEvent : ActionEvent
    {
        private const float DefaultHitStopDuration = 0.06f;

        [SerializeField, Min(0f)] private float damageMultiplier = 1f;
        [Tooltip("명중 시 공격자·피격자 애니메이션 정지 시간 (초)")]
        [SerializeField, Min(0f)] private float hitStopDuration = DefaultHitStopDuration;

        public HitWindowEvent() { }

        public HitWindowEvent(int startFrame, int endFrame, float damageMultiplier) : base(startFrame, endFrame)
        {
            this.damageMultiplier = damageMultiplier;
        }

        public override void OnBegin(IActionContext context) => context.BeginHit(damageMultiplier, hitStopDuration);
        public override void OnTick(IActionContext context) => context.TickHit();
        public override void OnEnd(IActionContext context) => context.EndHit();
    }
}
