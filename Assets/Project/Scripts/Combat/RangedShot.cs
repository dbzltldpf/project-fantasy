using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 발사 정보 (발사 지점, 초기 속도, 데미지)
    public readonly struct RangedShot
    {
        public readonly Vector3 Origin;
        public readonly Vector3 LaunchVelocity;
        public readonly int Damage;
        public readonly GameObject Instigator;

        public RangedShot(Vector3 origin, Vector3 launchVelocity, int damage, GameObject instigator)
        {
            Origin = origin;
            LaunchVelocity = launchVelocity;
            Damage = damage;
            Instigator = instigator;
        }
    }
}
