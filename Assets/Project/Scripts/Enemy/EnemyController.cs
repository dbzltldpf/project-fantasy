using System;
using System.Diagnostics;
using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProjectFantasy.Enemy
{
    // 적 컴포넌트 조립과 상태 머신 구동, 스폰·리스폰·전투 판단 공용 규칙
    [RequireComponent(typeof(EnemyNavigator), typeof(EnemyAnimator), typeof(EnemyPerception))]
    [RequireComponent(typeof(EnemyLoadout), typeof(Health), typeof(HitStop))]
    [RequireComponent(typeof(KillReward))]
    [DisallowMultipleComponent]
    public sealed class EnemyController : MonoBehaviour
    {
        private const float HalfAngleDivisor = 2f;

        [SerializeField] private EnemyData data;

        private StateMachine<EnemyStateBase> stateMachine;
        private Health health;
        private HitStop hitStop;
        private KillReward killReward;
        private Collider[] colliders;
        private bool isSpawned;
        private float nextAttackTime;
        private float nextGuardTime;
        private float staggerImmuneEndTime;

        public EnemyData Data => data;
        public EnemyNavigator Navigator { get; private set; }
        public EnemyAnimator EnemyAnimator { get; private set; }
        public EnemyPerception Perception { get; private set; }
        public EnemyLoadout Loadout { get; private set; }
        public MeleeAttacker Attacker { get; private set; }
        public Health Health => health;
        public Vector3 HomePosition { get; private set; }

        public bool IsBeyondLeash => (transform.position - HomePosition).sqrMagnitude > data.LeashRange * data.LeashRange;
        public bool CanGuard => data.HasShield && Time.time >= nextGuardTime;
        private bool CanAttack => data.ComboData != null && data.ComboData.ActionCount > 0 && Time.time >= nextAttackTime;

        public EnemySpawnState SpawnState { get; private set; }
        public EnemyPatrolState PatrolState { get; private set; }
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyAttackState AttackState { get; private set; }
        public EnemyGuardState GuardState { get; private set; }
        public EnemyHitState HitState { get; private set; }
        public EnemyReturnState ReturnState { get; private set; }
        public EnemyDeadState DeadState { get; private set; }

        // 시체 유지가 끝나 비활성화됨 (스포너가 리스폰 예약)
        public event Action<EnemyController> Despawned;

        private void Awake()
        {
            Navigator = GetComponent<EnemyNavigator>();
            EnemyAnimator = GetComponent<EnemyAnimator>();
            Perception = GetComponent<EnemyPerception>();
            Loadout = GetComponent<EnemyLoadout>();
            Attacker = GetComponent<MeleeAttacker>();
            health = GetComponent<Health>();
            hitStop = GetComponent<HitStop>();
            killReward = GetComponent<KillReward>();
            colliders = GetComponents<Collider>();

            health.SetMaxHealth(data.MaxHealth);
            killReward.SetExperience(data.Experience);
            Loadout.Apply(data);
            EnemyAnimator.Initialize(data.AnimationData, data.Weapon != null ? data.Weapon.IdleStateName : null);
            Perception.Configure(data.SightRange, data.SightAngle, data.LoseRange, data.ProvokedDuration);

            CreateStates();
            ValidateStates();
        }

        private void OnEnable()
        {
            health.Damaged += HandleDamaged;
            health.Blocked += HandleBlocked;
            health.Died += HandleDied;
        }

        private void OnDisable()
        {
            health.Damaged -= HandleDamaged;
            health.Blocked -= HandleBlocked;
            health.Died -= HandleDied;
        }

        // 씬에 직접 배치한 적은 제자리에서 스폰 (스포너가 생성한 적은 이미 스폰됨)
        private void Start()
        {
            if (!isSpawned) Spawn(transform.position, transform.rotation);
        }

        // 히트스톱 중에는 상태 시계·이동 모두 정지
        private void Update()
        {
            bool isFrozen = hitStop.IsActive;
            Navigator.SetPaused(isFrozen);
            if (isFrozen) return;

            float deltaTime = Time.deltaTime;
            if (health.IsAlive) Perception.Tick();
            stateMachine.Tick(deltaTime);
            Navigator.Tick(deltaTime);
        }

        public void ChangeState(EnemyStateBase nextState) => stateMachine.ChangeState(nextState);

        // 스폰·리스폰: 체력·보상 기록·타이머 초기화 후 등장 모션
        public void Spawn(Vector3 position, Quaternion rotation)
        {
            isSpawned = true;
            HomePosition = position;
            ResetCombat();
            SetCollidersEnabled(true);
            Navigator.Warp(position, rotation);

            if (stateMachine.CurrentState == null) stateMachine.Initialize(SpawnState);
            else stateMachine.ChangeState(SpawnState);
        }

        public void Despawn()
        {
            gameObject.SetActive(false);
            Despawned?.Invoke(this);
        }

        // 귀환·리스폰 시 전투 흔적 제거 (체력·기여자·대상)
        public void ResetCombat()
        {
            health.RestoreFull();
            killReward.ClearContributors();
            Perception.ClearTarget();
            nextAttackTime = 0f;
            nextGuardTime = 0f;
            staggerImmuneEndTime = 0f;
        }

        // 공격 거리 안: 막기(확률) 또는 정면이면 공격
        public void TryStartCombatAction()
        {
            if (CanGuard && Random.value < data.Guard.GuardChance)
            {
                ChangeState(GuardState);
                return;
            }

            if (CanAttack && IsFacingTarget()) ChangeState(AttackState);
        }

        public void StartCounter(ActionData action)
        {
            AttackState.SetCounter(action);
            ChangeState(AttackState);
        }

        // 공격·막기·경직이 끝난 뒤 대상 유무에 따라 복귀
        public void ReturnToCombat() => ChangeState(Perception.HasTarget ? (EnemyStateBase)ChaseState : PatrolState);

        public void StartAttackCooldown() => nextAttackTime = Time.time + data.RollAttackCooldown();
        public void StartGuardCooldown() => nextGuardTime = Time.time + data.Guard.GuardCooldown;

        public void SetCollidersEnabled(bool isEnabled)
        {
            foreach (Collider hitCollider in colliders)
            {
                hitCollider.enabled = isEnabled;
            }
        }

        private bool IsFacingTarget()
        {
            return Vector3.Angle(transform.forward, Perception.GetDirectionToTarget()) <= data.AttackAngle / HalfAngleDivisor;
        }

        private void CreateStates()
        {
            SpawnState = new EnemySpawnState(this);
            PatrolState = new EnemyPatrolState(this);
            ChaseState = new EnemyChaseState(this);
            AttackState = new EnemyAttackState(this);
            GuardState = new EnemyGuardState(this);
            HitState = new EnemyHitState(this);
            ReturnState = new EnemyReturnState(this);
            DeadState = new EnemyDeadState(this);
            stateMachine = new StateMachine<EnemyStateBase>();
        }

        // 경직 면역 중에는 피해만 받고 행동 유지 (무한 경직 방지)
        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (!health.IsAlive) return;

            Perception.NotifyAttacked(damageInfo.Instigator);
            if (Time.time < staggerImmuneEndTime || stateMachine.CurrentState == SpawnState) return;

            staggerImmuneEndTime = Time.time + data.HitReaction.StunDuration + data.StaggerImmunity;
            HitState.SetDamageInfo(damageInfo);
            ChangeState(HitState);
        }

        private void HandleBlocked(DamageInfo damageInfo)
        {
            Perception.NotifyAttacked(damageInfo.Instigator);
            if (stateMachine.CurrentState == GuardState) GuardState.OnBlocked(damageInfo);
        }

        private void HandleDied() => ChangeState(DeadState);

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void ValidateStates()
        {
            if (data.HitReaction == null) UnityEngine.Debug.LogError($"[{name}] '{data.name}'에 Hit Reaction이 없습니다.", data);

            foreach (string state in data.AnimationData.GetBaseStates())
            {
                EnemyAnimator.ValidateState(state);
            }

            if (data.Weapon != null) EnemyAnimator.ValidateState(data.Weapon.IdleStateName);
            if (data.Guard.CounterAction != null && data.HasShield) EnemyAnimator.ValidateState(data.Guard.CounterAction.StateName);
            if (data.ComboData == null) return;

            for (int i = 0; i < data.ComboData.ActionCount; i++)
            {
                ActionData action = data.ComboData.GetAction(i);
                if (action != null) EnemyAnimator.ValidateState(action.StateName);
            }
        }
    }
}
