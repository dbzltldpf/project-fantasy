using System;
using System.Diagnostics;
using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using ProjectFantasy.Magic;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 플레이어 컴포넌트 조립과 상태 머신 구동
    [RequireComponent(typeof(PlayerInputHandler), typeof(PlayerMotor), typeof(PlayerAnimator))]
    [RequireComponent(typeof(MeleeAttacker), typeof(Health), typeof(PlayerLoadout))]
    [RequireComponent(typeof(ShieldGuard), typeof(PlayerRangedWeapon), typeof(RangedAttacker))]
    [RequireComponent(typeof(PlayerAmmoVisual), typeof(PlayerMagicCaster), typeof(SpellCaster))]
    [DisallowMultipleComponent]
    public sealed class PlayerController : MonoBehaviour
    {
        private const float MaxMoveInputMagnitude = 1f;
        private const int InvalidFrame = -1;

        [SerializeField] private Transform cameraTransform;
        [Tooltip("조준 지점이 이 거리보다 가깝거나 뒤쪽이면 카메라 정면을 바라봄")]
        [SerializeField, Min(0f)] private float minAimFacingDistance = 1.5f;
        [SerializeField] private HitReactionData hitReactionData;
        [Tooltip("조준 모드 예상 경로 표시 (선택)")]
        [SerializeField] private TrajectoryPreview trajectoryPreview;

        private StateMachine<PlayerStateBase> stateMachine;
        private Health health;
        private Vector3 cachedAimPoint;
        private int cachedAimFrame = InvalidFrame;

        public PlayerInputHandler InputHandler { get; private set; }
        public PlayerMotor Motor { get; private set; }
        public PlayerAnimator PlayerAnimator { get; private set; }
        public MeleeAttacker Attacker { get; private set; }
        public PlayerLoadout Loadout { get; private set; }
        public ShieldGuard ShieldGuard { get; private set; }
        public PlayerRangedWeapon RangedWeapon { get; private set; }
        public RangedAttacker RangedAttacker { get; private set; }
        public PlayerAmmoVisual AmmoVisual { get; private set; }
        public PlayerMagicCaster MagicCaster { get; private set; }
        public SpellCaster SpellCaster { get; private set; }
        public HitReactionData HitReactionData => hitReactionData;
        public TrajectoryPreview TrajectoryPreview => trajectoryPreview;

        public AttackComboData AttackComboData => Loadout.CurrentWeapon != null ? Loadout.CurrentWeapon.ComboData : null;
        public bool CanAttack => AttackComboData != null && AttackComboData.StepCount > 0;
        public bool IsAimViewActive { get; private set; }

        public PlayerLocomotionState LocomotionState { get; private set; }
        public PlayerAirState AirState { get; private set; }
        public PlayerAttackState AttackState { get; private set; }
        public PlayerGuardState GuardState { get; private set; }
        public PlayerAimState AimState { get; private set; }
        public PlayerRangedFireState RangedFireState { get; private set; }
        public PlayerReloadState ReloadState { get; private set; }
        public PlayerCastState CastState { get; private set; }
        public PlayerSpellTargetState SpellTargetState { get; private set; }
        public PlayerHitState HitState { get; private set; }
        public PlayerDeadState DeadState { get; private set; }

        // 숄더뷰/조준점 전환 알림 (카메라·UI는 PlayerAimPresenter가 연결)
        public event Action<bool> AimViewChanged;

        private void Awake()
        {
            InputHandler = GetComponent<PlayerInputHandler>();
            Motor = GetComponent<PlayerMotor>();
            PlayerAnimator = GetComponent<PlayerAnimator>();
            Attacker = GetComponent<MeleeAttacker>();
            Loadout = GetComponent<PlayerLoadout>();
            ShieldGuard = GetComponent<ShieldGuard>();
            RangedWeapon = GetComponent<PlayerRangedWeapon>();
            RangedAttacker = GetComponent<RangedAttacker>();
            AmmoVisual = GetComponent<PlayerAmmoVisual>();
            MagicCaster = GetComponent<PlayerMagicCaster>();
            SpellCaster = GetComponent<SpellCaster>();
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
            stateMachine.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            health.Damaged -= HandleDamaged;
            health.Blocked -= HandleBlocked;
            health.Died -= HandleDied;
            Loadout.WeaponChanged -= HandleWeaponChanged;
            stateMachine.StateChanged -= HandleStateChanged;
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

            Vector3 right = cameraTransform.right;
            right.y = 0f;

            Vector3 move = GetCameraFlatForward() * input.y + right.normalized * input.x;
            return Vector3.ClampMagnitude(move, MaxMoveInputMagnitude);
        }

        // 카메라가 보는 수평 방향 (조준·발사 시 캐릭터 정면)
        public Vector3 GetCameraFlatForward()
        {
            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > Mathf.Epsilon ? forward.normalized : transform.forward;
        }

        // 화면 중앙 조준 레이
        public Ray GetAimRay() => new Ray(cameraTransform.position, cameraTransform.forward);

        // 조준점이 가리키는 월드 지점 (프레임당 레이캐스트 1회로 캐싱)
        public Vector3 GetAimPoint()
        {
            if (cachedAimFrame != Time.frameCount)
            {
                cachedAimPoint = RangedAttacker.ResolveAimPoint(GetAimRay());
                cachedAimFrame = Time.frameCount;
            }
            return cachedAimPoint;
        }

        // 조준 지점을 향하는 수평 방향 (몸·활·조준선 일치), 너무 가깝거나 뒤쪽이면 카메라 정면
        public Vector3 GetAimFacingDirection()
        {
            Vector3 cameraForward = GetCameraFlatForward();
            Vector3 toAimPoint = GetAimPoint() - transform.position;
            toAimPoint.y = 0f;

            bool isTooClose = toAimPoint.sqrMagnitude < minAimFacingDistance * minAimFacingDistance;
            bool isBehind = Vector3.Dot(toAimPoint, cameraForward) <= 0f;
            return isTooClose || isBehind ? cameraForward : toAimPoint.normalized;
        }

        public Vector3 GetMuzzlePosition() => transform.TransformPoint(RangedWeapon.Current.MuzzleOffset);

        // 조준점에 맞는 발사 속도 계산 (미리보기·발사 공용)
        public bool TryResolveLaunch(out Vector3 origin, out Vector3 launchVelocity, out ProjectileProfile profile)
        {
            origin = default;
            launchVelocity = default;
            if (!RangedWeapon.TryGetProjectileProfile(out profile)) return false;

            origin = GetMuzzlePosition();
            launchVelocity = RangedAttacker.ResolveLaunchVelocity(origin, GetAimPoint(), profile);
            return true;
        }

        private void CreateStates()
        {
            LocomotionState = new PlayerLocomotionState(this);
            AirState = new PlayerAirState(this);
            AttackState = new PlayerAttackState(this);
            GuardState = new PlayerGuardState(this);
            AimState = new PlayerAimState(this);
            RangedFireState = new PlayerRangedFireState(this);
            ReloadState = new PlayerReloadState(this);
            CastState = new PlayerCastState(this);
            SpellTargetState = new PlayerSpellTargetState(this);
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

            if (weapon is RangedWeaponData ranged)
            {
                PlayerAnimator.ValidateState(ranged.FireStateName);
                if (ranged.HasWindup) PlayerAnimator.ValidateState(ranged.WindupStateName);
                if (ranged.CanAim) PlayerAnimator.ValidateState(ranged.AimIdleStateName);
                if (ranged.RequiresReload) PlayerAnimator.ValidateState(ranged.ReloadStateName);
                return;
            }

            if (weapon is MagicWeaponData magic)
            {
                ValidateSpell(magic);
                return;
            }

            if (weapon.ComboData == null) return;

            for (int i = 0; i < weapon.ComboData.StepCount; i++)
            {
                PlayerAnimator.ValidateState(weapon.ComboData.GetStep(i).StateName);
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void ValidateSpell(MagicWeaponData weapon)
        {
            if (weapon.Spell == null)
            {
                UnityEngine.Debug.LogError($"[{nameof(PlayerController)}] '{weapon.name}'에 마법(Spell)이 지정되지 않았습니다.", weapon);
                return;
            }

            PlayerAnimator.ValidateState(weapon.Spell.CastStateName);
            if (weapon.Spell is AreaSpellData area) PlayerAnimator.ValidateState(area.TargetingIdleStateName);
        }

        private void HandleStateChanged(PlayerStateBase previousState, PlayerStateBase nextState)
        {
            if (nextState.UsesAimView == IsAimViewActive) return;

            IsAimViewActive = nextState.UsesAimView;
            AimViewChanged?.Invoke(IsAimViewActive);
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
