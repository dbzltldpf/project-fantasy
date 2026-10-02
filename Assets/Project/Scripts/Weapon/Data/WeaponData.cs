using System;
using ProjectFantasy.Combat;
using UnityEngine;

namespace ProjectFantasy.Weapon
{
    // 무기 종류·쥐는 손·공격력·콤보·판정 범위·대기 모션
    [CreateAssetMenu(fileName = "WeaponData", menuName = "ProjectFantasy/Weapon/Weapon Data")]
    public class WeaponData : EquipmentData
    {
        [Header("Type")]
        [SerializeField] private WeaponType weaponType = WeaponType.OneHanded;
        [SerializeField] private EquipHand gripHand = EquipHand.Right;
        [Tooltip("보조 손(왼손)까지 사용하는 무기 (양손검, 양손 석궁 등)")]
        [SerializeField] private bool occupiesOffHand;
        [Tooltip("보조 손에 방패 장착 허용 (Staff는 마법서만)")]
        [SerializeField] private bool allowsShield = true;

        [Header("Combat")]
        [SerializeField, Min(0)] private int attackPower = 10;
        [SerializeField] private AttackComboData comboData;
        [SerializeField] private Vector3 hitOffset = new Vector3(0f, 1f, 1f);
        [SerializeField, Min(0f)] private float hitRadius = 0.8f;

        [Header("Blade Trace")]
        [Tooltip("무기 모델 로컬 기준 칼날 시작/끝 (같으면 위의 구체 판정 사용)")]
        [SerializeField] private Vector3 bladeBase;
        [SerializeField] private Vector3 bladeTip;
        [SerializeField, Min(0f)] private float bladeRadius = 0.1f;

        [Header("Animation")]
        [SerializeField] private string idleStateName = "Idle_A";

        [NonSerialized] private int idleStateHash;
        [NonSerialized] private bool isHashCached;

        public WeaponType WeaponType => weaponType;
        public EquipHand GripHand => gripHand;
        public bool IsHeldInLeftHand => gripHand == EquipHand.Left;
        public bool OccupiesOffHand => occupiesOffHand || IsHeldInLeftHand;
        public bool AllowsShield => allowsShield;
        public bool IsMagic => weaponType == WeaponType.Wand || weaponType == WeaponType.Staff;
        public bool HasMeleeHit => weaponType == WeaponType.Unarmed || weaponType == WeaponType.OneHanded || weaponType == WeaponType.TwoHanded;
        public int AttackPower => attackPower;
        public AttackComboData ComboData => comboData;
        public Vector3 HitOffset => hitOffset;
        public float HitRadius => hitRadius;
        public bool HasBlade => ModelPrefab != null && bladeBase != bladeTip && bladeRadius > 0f;
        public Vector3 BladeBase => bladeBase;
        public Vector3 BladeTip => bladeTip;
        public float BladeRadius => bladeRadius;
        public string IdleStateName => idleStateName;

        public int IdleStateHash
        {
            get
            {
                if (!isHashCached)
                {
                    idleStateHash = Animator.StringToHash(idleStateName);
                    isHashCached = true;
                }
                return idleStateHash;
            }
        }

        protected virtual void OnValidate() => isHashCached = false;
    }
}
