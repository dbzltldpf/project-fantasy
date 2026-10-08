using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace ProjectFantasy.Enemy
{
    // 원거리 공통: 공격 거리 안이면 거리와 무관하게 공격, 사선 필요, 대상 몸통 조준 + 탄 퍼짐 (이동 예측 없음 → 회피 가능)
    [Serializable]
    public abstract class EnemyRangedStyle : EnemyCombatStyle
    {
        [Tooltip("공격 거리 (이보다 멀면 접근, 가까우면 제자리에서 공격, m)")]
        [SerializeField, Min(0f), FormerlySerializedAs("maxRange")] private float attackRange = 14f;
        [Tooltip("조준 높이 (대상 발 기준, m)")]
        [SerializeField, Min(0f)] private float targetHeight = 1.2f;
        [Tooltip("탄 퍼짐 (도, 클수록 잘 빗나감)")]
        [SerializeField, Min(0f)] private float spreadAngle = 3f;

        public override float GetAttackRange(EnemyData data) => attackRange;
        public override bool RequiresLineOfSight => true;

        public Vector3 GetAimPoint(Transform target) => target.position + Vector3.up * targetHeight;

        // 발사 방향을 원뿔 안에서 무작위로 흔듦 (속력 유지)
        public Vector3 ApplySpread(Vector3 velocity)
        {
            if (spreadAngle <= 0f || velocity.sqrMagnitude <= Mathf.Epsilon) return velocity;

            Vector2 offset = Random.insideUnitCircle * spreadAngle;
            Quaternion spread = Quaternion.LookRotation(velocity) * Quaternion.Euler(offset.y, offset.x, 0f);
            return spread * Vector3.forward * velocity.magnitude;
        }
    }
}
