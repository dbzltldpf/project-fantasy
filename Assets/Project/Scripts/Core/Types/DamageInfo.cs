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
        public readonly DamageType Type;

        public DamageInfo(int amount, Vector3 hitPoint, Vector3 direction, GameObject instigator, DamageType type = DamageType.Physical)
        {
            Amount = amount;
            HitPoint = hitPoint;
            Direction = direction;
            Instigator = instigator;
            Type = type;
        }

        // 방어력 적용 후 피해량만 교체
        public DamageInfo WithAmount(int amount) => new DamageInfo(amount, HitPoint, Direction, Instigator, Type);
    }
}
