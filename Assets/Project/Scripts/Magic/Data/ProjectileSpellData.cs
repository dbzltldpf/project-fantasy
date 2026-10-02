using ProjectFantasy.Combat;
using UnityEngine;

namespace ProjectFantasy.Magic
{
    // 직선 마법탄 (중력 없음, 명중 시 소멸) - Wand 단일 공격
    [CreateAssetMenu(fileName = "ProjectileSpell", menuName = "ProjectFantasy/Magic/Projectile Spell")]
    public sealed class ProjectileSpellData : SpellData
    {
        private const float NoGravity = 0f;
        private const bool SticksOnHit = false;
        private const float NoStickDuration = 0f;

        [Header("Projectile")]
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField, Min(0f)] private float speed = 30f;
        [SerializeField, Min(0f)] private float lifetime = 3f;

        [Header("Impact")]
        [Tooltip("1회 재생 이펙트 (Looping 끈 Prefab Variant)")]
        [SerializeField] private GameObject impactEffectPrefab;

        public override bool RequiresTargeting => false;

        public ProjectileProfile ToProfile()
        {
            return new ProjectileProfile(projectilePrefab, speed, NoGravity, lifetime, SticksOnHit, NoStickDuration, impactEffectPrefab);
        }
    }
}
