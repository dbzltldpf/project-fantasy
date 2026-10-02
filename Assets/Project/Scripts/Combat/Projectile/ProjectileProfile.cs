using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 투사체 발사 설정 (화살: 중력 + 박힘, 마법탄: 중력 0 + 명중 시 소멸)
    public readonly struct ProjectileProfile
    {
        public readonly Projectile Prefab;
        public readonly float Speed;
        public readonly float Gravity;
        public readonly float Lifetime;
        public readonly bool StickOnHit;
        public readonly float StickDuration;
        public readonly GameObject ImpactEffectPrefab;

        public ProjectileProfile(Projectile prefab, float speed, float gravity, float lifetime, bool stickOnHit, float stickDuration,
            GameObject impactEffectPrefab)
        {
            Prefab = prefab;
            Speed = speed;
            Gravity = gravity;
            Lifetime = lifetime;
            StickOnHit = stickOnHit;
            StickDuration = stickDuration;
            ImpactEffectPrefab = impactEffectPrefab;
        }
    }
}
