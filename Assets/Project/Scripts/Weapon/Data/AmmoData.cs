using ProjectFantasy.Combat;
using ProjectFantasy.Items;
using UnityEngine;

namespace ProjectFantasy.Weapon
{
    // 화살/볼트 종류 (모델·그립은 시위에 건 화살 표시, 투사체 설정은 발사에 사용)
    [CreateAssetMenu(fileName = "AmmoData", menuName = "ProjectFantasy/Weapon/Ammo Data")]
    public sealed class AmmoData : EquipmentData
    {
        private const bool SticksOnHit = true;
        private const int AmmoMaxStack = 99;

        [Header("Projectile")]
        [Tooltip("날아가는 화살 프리팹 (Projectile)")]
        [SerializeField] private Projectile projectilePrefab;
        [Tooltip("화살 중력 (m/s², 클수록 빨리 떨어짐)")]
        [SerializeField, Min(0f)] private float gravity = 9.81f;
        [Tooltip("최대 비행 시간 (초)")]
        [SerializeField, Min(0f)] private float lifetime = 5f;
        [Tooltip("박힌 뒤 사라질 때까지 시간")]
        [SerializeField, Min(0f)] private float stickDuration = 10f;

        [Header("Impact")]
        [Tooltip("1회 재생 이펙트 (Looping 끈 Prefab Variant)")]
        [SerializeField] private GameObject impactEffectPrefab;

        public override ItemCategory Category => ItemCategory.Ammo;
        protected override int DefaultMaxStack => AmmoMaxStack;

        public Projectile ProjectilePrefab => projectilePrefab;

        // 화살은 중력 + 명중 시 박힘
        public ProjectileProfile ToProfile(float launchSpeed)
        {
            return new ProjectileProfile(projectilePrefab, launchSpeed, gravity, lifetime, SticksOnHit, stickDuration, impactEffectPrefab);
        }
    }
}
