using UnityEngine;

namespace ProjectFantasy.Weapon
{
    // 방패 가드 범위와 막기 반응 (보조 손이 비어 있는 무기와 장착)
    [CreateAssetMenu(fileName = "ShieldData", menuName = "ProjectFantasy/Weapon/Shield Data")]
    public sealed class ShieldData : OffHandData
    {
        [Header("Guard")]
        [SerializeField, Range(0f, 360f)] private float guardAngle = 120f;
        [SerializeField, Min(0f)] private float blockKnockbackSpeed = 2f;
        [SerializeField, Min(0f)] private float blockStunDuration = 0.3f;

        public float GuardAngle => guardAngle;
        public float BlockKnockbackSpeed => blockKnockbackSpeed;
        public float BlockStunDuration => blockStunDuration;

        public override bool CanEquipWith(WeaponData weapon) => !weapon.OccupiesOffHand;
    }
}
