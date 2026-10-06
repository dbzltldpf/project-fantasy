using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 타격 순간 애니메이션 일시 정지 (공격자·피격자 공용, 로직은 IsActive로 정지 여부 확인)
    [DisallowMultipleComponent]
    public sealed class HitStop : MonoBehaviour
    {
        private const float FrozenSpeed = 0f;

        [Tooltip("멈출 애니메이터 (비우면 자식 자동 탐색)")]
        [SerializeField] private Animator animator;

        private float endTime;
        private float resumeSpeed;
        private bool isFrozen;

        public bool IsActive => Time.time < endTime;

        private void Reset() => animator = GetComponentInChildren<Animator>();

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (isFrozen && !IsActive) Resume();
        }

        private void OnDisable()
        {
            endTime = 0f;
            if (isFrozen) Resume();
        }

        // 겹쳐 호출되면 더 늦게 끝나는 쪽으로 연장
        public void Apply(float duration)
        {
            if (duration <= 0f) return;

            endTime = Mathf.Max(endTime, Time.time + duration);
            if (isFrozen || animator == null) return;

            resumeSpeed = animator.speed;
            animator.speed = FrozenSpeed;
            isFrozen = true;
        }

        private void Resume()
        {
            animator.speed = resumeSpeed;
            isFrozen = false;
        }
    }
}
