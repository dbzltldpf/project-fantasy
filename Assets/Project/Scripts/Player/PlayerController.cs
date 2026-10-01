using System.Diagnostics;
using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 플레이어 컴포넌트 조립과 상태 머신 구동
    [RequireComponent(typeof(PlayerInputHandler), typeof(PlayerMotor), typeof(PlayerAnimator))]
    [RequireComponent(typeof(MeleeAttacker), typeof(Health), typeof(PlayerLoadout))]
    [RequireComponent(typeof(ShieldGuard))]
    [DisallowMultipleComponent]
    public sealed class PlayerController : MonoBehaviour
    {
        private const float MaxMoveInputMagnitude = 1f;

        [SerializeField] private Transform cameraTransform;
        [SerializeField] private HitReactionData hitReactionData;

        private StateMachine<PlayerStateBase> stateMachine;
        private Health health;

        public PlayerInputHandler InputHandler { get; private set; }
        public PlayerMotor Motor { get; private set; }
        public PlayerAnimator PlayerAnimator { get; private set; }
        public MeleeAttacker Attacker { get; private set; }
        public PlayerLoadout Loadout { get; private set; }
        public ShieldGuard ShieldGuard { get; private set; }
        public HitReactionData HitReactionData => hitReactionData;

        public AttackComboData AttackComboData => Loadout.CurrentWeapon != null ? Loadout.CurrentWeapon.ComboData : null;
        public bool CanAttack => AttackComboData != null && AttackComboData.StepCount > 0;

        public PlayerLocomotionState LocomotionState { get; private set; }
        public PlayerAirState AirState { get; private set; }
        public PlayerAttackState AttackState { get; private set; }
        public PlayerGuardState GuardState { get; private set; }
        public PlayerHitState HitState { get; private set; }
        public PlayerDeadState DeadState { get; private set; }

        private void Awake()
        {
            InputHandler = GetComponent<PlayerInputHandler>();
            Motor = GetComponent<PlayerMotor>();
            PlayerAnimator = GetComponent<PlayerAnimator>();
            Attacker = GetComponent<MeleeAttacker>();
            Loadout = GetComponent<PlayerLoadout>();
            ShieldGuard = GetComponent<ShieldGuard>();
            health = GetComponent<Health>();

            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;

            CreateStates();
        }

        private void OnEnable()
        {
            health.Damaged += HandleDamaged;
            health.Blocked += HandleBlocked;
            health.Died += HandleDied;
            Loadout.WeaponChanged += HandleWeaponChanged;
        }

        private void OnDisable()
        {
            health.Damaged -= HandleDamaged;
            health.Blocked -= HandleBlocked;
            health.Died -= HandleDied;
            Loadout.WeaponChanged -= HandleWeaponChanged;
        }

        private void Start()
        {
            ValidateWeaponStates();
            stateMachine.Initialize(LocomotionState);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            stateMachine.Tick(deltaTime);
            Motor.Tick(deltaTime);
        }

        public void ChangeState(PlayerStateBase nextState) => stateMachine.ChangeState(nextState);

        // 카메라 기준 수평 이동 벡터 (크기 = 입력 강도)
        public Vector3 GetCameraRelativeMove()
        {
            Vector2 input = InputHandler.MoveInput;
            if (input.sqrMagnitude <= Mathf.Epsilon) return Vector3.zero;

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;

            Vector3 move = forward.normalized * input.y + right.normalized * input.x;
            return Vector3.ClampMagnitude(move, MaxMoveInputMagnitude);
        }

        private void CreateStates()
        {
            LocomotionState = new PlayerLocomotionState(this);
            AirState = new PlayerAirState(this);
            AttackState = new PlayerAttackState(this);
            GuardState = new PlayerGuardState(this);
            HitState = new PlayerHitState(this);
            DeadState = new PlayerDeadState(this);
            stateMachine = new StateMachine<PlayerStateBase>();
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void ValidateWeaponStates()
        {
            ValidateWeapon(Loadout.UnarmedWeapon);
            foreach (WeaponData weapon in Loadout.OwnedWeapons)
            {
                ValidateWeapon(weapon);
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void ValidateWeapon(WeaponData weapon)
        {
            if (weapon == null)
            {
                UnityEngine.Debug.LogError($"[{nameof(PlayerController)}] 맨손(Unarmed) 무기 데이터가 지정되지 않았습니다.", this);
                return;
            }

            PlayerAnimator.ValidateState(weapon.IdleStateName);
            if (weapon.ComboData == null) return;

            for (int i = 0; i < weapon.ComboData.StepCount; i++)
            {
                PlayerAnimator.ValidateState(weapon.ComboData.GetStep(i).StateName);
            }
        }

        private void HandleWeaponChanged(WeaponData weapon) => PlayerAnimator.SetIdleState(weapon.IdleStateHash);

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (!health.IsAlive) return;

            HitState.SetDamageInfo(damageInfo);
            ChangeState(HitState);
        }

        private void HandleBlocked(DamageInfo damageInfo) => GuardState.OnBlocked(damageInfo);

        private void HandleDied() => ChangeState(DeadState);
    }
}
