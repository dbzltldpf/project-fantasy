using UnityEngine;

namespace ProjectFantasy.Core
{
    // 피해 전달용 값 타입 (GC 할당 없음)
    public readonly struct DamageInfo
    {
        public readonly int Amount;
        public readonly Vector3 HitPoint;
        public readonly Vector3 Direction;
        public readonly GameObject Instigator;

        public DamageInfo(int amount, Vector3 hitPoint, Vector3 direction, GameObject instigator)
        {
            Amount = amount;
            HitPoint = hitPoint;
            Direction = direction;
            Instigator = instigator;
        }
    }
}
