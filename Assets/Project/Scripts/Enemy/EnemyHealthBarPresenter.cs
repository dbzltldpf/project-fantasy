using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using ProjectFantasy.UI;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 적 체력바 표시 규칙: 한 번 맞으면 전투가 끝날 때(사망·대상 상실)까지 표시, 그 뒤 일정 시간 후 숨김
    // 체력바 프리팹(EnemyHealthBarView와 같은 오브젝트)에 두고 부모의 Health·EnemyPerception 사용
    [RequireComponent(typeof(EnemyHealthBarView))]
    [DisallowMultipleComponent]
    public sealed class EnemyHealthBarPresenter : MonoBehaviour
    {
        [Tooltip("전투가 끝난 뒤 체력바를 유지하는 시간 (초)")]
        [SerializeField, Min(0f)] private float hideDelay = 4f;

        private EnemyHealthBarView view;
        private Health health;
        private EnemyPerception perception;
        private bool isEngaged;

        private void Awake()
        {
            view = GetComponent<EnemyHealthBarView>();
            health = GetComponentInParent<Health>();
            perception = GetComponentInParent<EnemyPerception>();
        }

        // 리스폰 시 숨긴 상태로 시작
        private void OnEnable()
        {
            health.HealthChanged += HandleHealthChanged;
            health.Damaged += HandleDamaged;
            health.Died += EndEngagement;
            if (perception != null) perception.TargetChanged += HandleTargetChanged;

            isEngaged = false;
            view.HideImmediate();
            HandleHealthChanged(health.CurrentHealth, health.MaxHealth);
        }

        private void OnDisable()
        {
            health.HealthChanged -= HandleHealthChanged;
            health.Damaged -= HandleDamaged;
            health.Died -= EndEngagement;
            if (perception != null) perception.TargetChanged -= HandleTargetChanged;
        }

        private void HandleHealthChanged(int current, int max) => view.SetRatio((float)current / max);

        private void HandleDamaged(DamageInfo _)
        {
            if (!health.IsAlive) return;

            isEngaged = true;
            view.Show();
        }

        // 맞은 뒤 대상을 잃으면 전투 종료, 숨김 대기 중(아직 보임) 다시 대상을 잡으면 계속 표시
        private void HandleTargetChanged(Transform target)
        {
            if (target == null)
            {
                if (isEngaged) EndEngagement();
                return;
            }

            if (!view.IsVisible) return;

            isEngaged = true;
            view.Show();
        }

        private void EndEngagement()
        {
            isEngaged = false;
            view.HideAfter(hideDelay);
        }
    }
}
