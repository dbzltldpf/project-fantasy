using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 가드 중 정면 각도 내 공격 방어
    [DisallowMultipleComponent]
    public sealed class ShieldGuard : MonoBehaviour, IDamageBlocker
    {
        private const float HalfAngleDivisor = 2f;

        [SerializeField] private Transform facingTransform;

        private float guardHalfAngle;

        public bool HasShield { get; private set; }
        public bool IsGuarding { get; private set; }

        private void Awake()
        {
            if (facingTransform == null) facingTransform = transform;
        }

        public void EnableShield(float guardAngle)
        {
            guardHalfAngle = guardAngle / HalfAngleDivisor;
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
    }
}
