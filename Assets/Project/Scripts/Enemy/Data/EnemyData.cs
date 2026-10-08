using ProjectFantasy.Combat;
using ProjectFantasy.Items;
using ProjectFantasy.Utils;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 적 한 종류: 능력치·장비·감지·이동·전투·막기·드랍·애니메이션
    [CreateAssetMenu(fileName = "EnemyData", menuName = "ProjectFantasy/Enemy/Enemy Data")]
    public sealed class EnemyData : ScriptableObject
    {
        // 전투 방식을 비워 둔 기존 적 데이터는 근접
        private static readonly MeleeCombatStyle DefaultMeleeStyle = new MeleeCombatStyle();

        [Header("Info")]
        [SerializeField] private string displayName = "Skeleton";
        [Tooltip("최대 체력")]
        [SerializeField, Min(1)] private int maxHealth = 60;
        [Tooltip("공격력 (피해 = 공격력 × 액션 피해 배율)")]
        [SerializeField, Min(0)] private int attackPower = 8;
        [Tooltip("처치 시 기여한 공격자 각각이 받는 경험치")]
        [SerializeField, Min(0)] private int experience = 30;

        [Header("Equipment")]
        [Tooltip("무기 (모델·콤보·칼날·대기 모션 재사용)")]
        [SerializeField] private WeaponData weapon;
        [Tooltip("방패 (비우면 없음, 있으면 막기 사용)")]
        [SerializeField] private ShieldData shield;
        [Tooltip("방패 방어력 (적은 고정값)")]
        [SerializeField, Min(0)] private int defense;
        [SerializeField, Min(0)] private int magicDefense;

        [Header("Perception")]
        [Tooltip("시야 거리 (m)")]
        [SerializeField, Min(0f)] private float sightRange = 10f;
        [Tooltip("시야각 (정면 기준 전체 각도)")]
        [SerializeField, Range(0f, 360f)] private float sightAngle = 120f;
        [Tooltip("대상이 이보다 멀어지면 추적 포기 (m)")]
        [SerializeField, Min(0f)] private float loseRange = 18f;
        [Tooltip("공격받은 뒤 이 시간 동안은 거리와 무관하게 공격자 추적 (초, 맞을 때마다 갱신)")]
        [SerializeField, Min(0f)] private float provokedDuration = 5f;
        [Tooltip("스폰 지점에서 이보다 멀어지면 귀환 (m)")]
        [SerializeField, Min(0f)] private float leashRange = 25f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float walkSpeed = 1.5f;
        [SerializeField, Min(0f)] private float runSpeed = 4f;
        [Tooltip("회전 속도 (도/초)")]
        [SerializeField, Min(0f)] private float turnSpeed = 540f;
        [Tooltip("스폰 지점 주변 배회 반경 (m)")]
        [SerializeField, Min(0f)] private float patrolRadius = 6f;
        [Tooltip("배회 지점 사이 대기 시간 범위 (초)")]
        [SerializeField] private FloatRange idleTimeRange = new FloatRange(2f, 5f);

        [Header("Combat")]
        [Tooltip("전투 방식 (비우면 근접): Melee / Shooter(활·석궁) / Caster(마법)")]
        [SerializeReference, SubclassSelector] private EnemyCombatStyle combatStyle;
        [Tooltip("대상이 정면 이 각도 안에 들어와야 공격 시작 (전체 각도)")]
        [SerializeField, Range(0f, 360f)] private float attackAngle = 40f;
        [Tooltip("공격 후 다음 공격까지 대기 시간 범위 (초)")]
        [SerializeField] private FloatRange attackCooldownRange = new FloatRange(1.2f, 2f);
        [Tooltip("경직 후 이 시간 동안은 다시 맞아도 경직되지 않음 (무한 경직 방지, 초)")]
        [SerializeField, Min(0f)] private float staggerImmunity = 1f;
        [Tooltip("피격 경직·넉백")]
        [SerializeField] private HitReactionData hitReaction;

        [Header("Melee (근접 방식)")]
        [Tooltip("이 거리 안이면 공격 (m)")]
        [SerializeField, Min(0f)] private float attackRange = 1.8f;
        [Tooltip("한 번에 이어 치는 최대 콤보 수 (무기 콤보 길이 이하)")]
        [SerializeField, Min(1)] private int maxComboActions = 2;

        [Header("Guard (근접 + 방패가 있을 때만)")]
        [SerializeField] private EnemyGuardSettings guard = new EnemyGuardSettings();

        [Header("Spawn & Death")]
        [Tooltip("등장 모션 시간 (초, 이 동안 행동 안 함)")]
        [SerializeField, Min(0f)] private float spawnDuration = 1.5f;
        [Tooltip("사망 후 시체 유지 시간 (초)")]
        [SerializeField, Min(0f)] private float corpseDuration = 3f;
        [Tooltip("처치 시 드랍 표 (비우면 드랍 없음)")]
        [SerializeField] private DropTable dropTable;

        [Header("Animation")]
        [SerializeField] private EnemyAnimationData animationData;

        public string DisplayName => displayName;
        public int MaxHealth => maxHealth;
        public int AttackPower => attackPower;
        public int Experience => experience;
        public WeaponData Weapon => weapon;
        public ShieldData Shield => shield;
        public int Defense => defense;
        public int MagicDefense => magicDefense;
        public bool HasShield => shield != null;
        public float SightRange => sightRange;
        public float SightAngle => sightAngle;
        public float LoseRange => loseRange;
        public float ProvokedDuration => provokedDuration;
        public float LeashRange => leashRange;
        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;
        public float TurnSpeed => turnSpeed;
        public float PatrolRadius => patrolRadius;
        public EnemyCombatStyle CombatStyle => combatStyle ?? DefaultMeleeStyle;
        public float AttackRange => attackRange;
        public float AttackAngle => attackAngle;
        public int MaxComboActions => maxComboActions;
        public float StaggerImmunity => staggerImmunity;
        public HitReactionData HitReaction => hitReaction;
        public EnemyGuardSettings Guard => guard;
        public float SpawnDuration => spawnDuration;
        public float CorpseDuration => corpseDuration;
        public DropTable DropTable => dropTable;
        public EnemyAnimationData AnimationData => animationData;
        public AttackComboData ComboData => weapon != null ? weapon.ComboData : null;

        public float RollIdleTime() => idleTimeRange.Random();
        public float RollAttackCooldown() => attackCooldownRange.Random();
    }
}
