using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 방패: 가드 중 정면 각도 내 공격 완전 방어, 장착 중에는 방어력·마법 방어력으로 피해 감소
    [DisallowMultipleComponent]
    public sealed class ShieldGuard : MonoBehaviour, IDamageBlocker, IDamageReducer
    {
        private const float HalfAngleDivisor = 2f;
        private const float DefenseScale = 100f;
        private const int MinDamage = 1;

        [Tooltip("가드 정면 기준 (비우면 자신)")]
        [SerializeField] private Transform facingTransform;

        private float guardHalfAngle;
        private int defense;
        private int magicDefense;

        public bool HasShield { get; private set; }
        public bool IsGuarding { get; private set; }

        private void Awake()
        {
            if (facingTransform == null) facingTransform = transform;
        }

        public void EnableShield(float guardAngle, int shieldDefense, int shieldMagicDefense)
        {
            guardHalfAngle = guardAngle / HalfAngleDivisor;
            defense = shieldDefense;
            magicDefense = shieldMagicDefense;
            HasShield = true;
        }

        public void DisableShield()
        {
            HasShield = false;
            IsGuarding = false;
        }

        public void SetGuarding(bool isGuarding) => IsGuarding = isGuarding && HasShield;

        public bool TryBlock(in DamageInfo damageInfo)
        {
            if (!IsGuarding) return false;

            // 가해자 방향 = 피격 방향의 반대
            Vector3 toAttacker = -damageInfo.Direction;
            toAttacker.y = 0f;
            if (toAttacker.sqrMagnitude <= Mathf.Epsilon) return true;

            return Vector3.Angle(facingTransform.forward, toAttacker) <= guardHalfAngle;
        }

        // 피해 × 100 / (100 + 방어력): 방어력이 높을수록 효율이 완만하게 감소
        public int Reduce(in DamageInfo damageInfo)
        {
            if (!HasShield) return damageInfo.Amount;

            int armor = damageInfo.Type == DamageType.Magic ? magicDefense : defense;
            float reduced = damageInfo.Amount * DefenseScale / (DefenseScale + armor);
            return Mathf.Max(MinDamage, Mathf.RoundToInt(reduced));
        }
    }
}
