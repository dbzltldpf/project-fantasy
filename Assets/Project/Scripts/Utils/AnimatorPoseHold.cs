using UnityEngine;

namespace ProjectFantasy.Utils
{
    // 상태가 지정 정규화 시간에 도달하면 Speed Multiplier 파라미터를 0으로 만들어 자세 고정 (조준 대기 등, 플레이어·적 공용)
    // 상태의 Speed Multiplier에 같은 Float 파라미터를 연결해야 동작
    public sealed class AnimatorPoseHold
    {
        private const int BaseLayerIndex = 0;
        private const int NoState = 0;
        private const float NormalSpeed = 1f;
        private const float FrozenSpeed = 0f;
        private const float NoHoldTime = 1f;

        private readonly Animator animator;
        private readonly int parameterHash;
        private int holdStateHash = NoState;
        private float holdNormalizedTime;
        private bool isFrozen;

        public bool IsAvailable { get; }

        public AnimatorPoseHold(Animator animator, int parameterHash)
        {
            this.animator = animator;
            this.parameterHash = parameterHash;
            IsAvailable = animator.HasFloatParameter(parameterHash);
        }

        // normalizedTime이 1 이상이면 고정하지 않음
        public void Hold(int stateHash, float normalizedTime)
        {
            if (!IsAvailable || normalizedTime >= NoHoldTime) return;

            holdStateHash = stateHash;
            holdNormalizedTime = normalizedTime;
        }

        // 다른 상태로 전환할 때 호출
        public void Release()
        {
            holdStateHash = NoState;
            if (!isFrozen) return;

            animator.SetFloat(parameterHash, NormalSpeed);
            isFrozen = false;
        }

        // 매 프레임: 대상 상태가 고정 시점에 도달하면 정지
        public void Tick()
        {
            if (holdStateHash == NoState || isFrozen || animator.IsInTransition(BaseLayerIndex)) return;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(BaseLayerIndex);
            if (stateInfo.shortNameHash != holdStateHash || stateInfo.normalizedTime < holdNormalizedTime) return;

            animator.SetFloat(parameterHash, FrozenSpeed);
            isFrozen = true;
        }
    }
}
