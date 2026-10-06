using System;
using ProjectFantasy.Combat;
using ProjectFantasy.Items;
using UnityEngine;

namespace ProjectFantasy.Weapon
{
    // 무기 종류·쥐는 손·공격력·콤보·판정 범위·대기 모션
    [CreateAssetMenu(fileName = "WeaponData", menuName = "ProjectFantasy/Weapon/Weapon Data")]
    public class WeaponData : EquipmentData
    {
        private const int DefaultAttackPower = 10;

        [Tooltip("무기 종류 (공격 방식·인스펙터 섹션 결정)")]
        [SerializeField] private WeaponType weaponType = WeaponType.OneHanded;
        [Tooltip("쥐는 손 (활만 Left)")]
        [SerializeField] private EquipHand gripHand = EquipHand.Right;
        [Tooltip("보조 손(왼손)까지 사용 → 보조 장비 장착 불가 (양손검, 양손 석궁)")]
        [SerializeField] private bool occupiesOffHand;
        [Tooltip("보조 손에 방패 허용 (Staff는 끔 → 마법서만)")]
        [SerializeField] private bool allowsShield = true;

        [Tooltip("개체 생성 시 굴리는 공격력 범위 (등급 배율 적용 전)")]
        [SerializeField] private StatRange attackPowerRange = new StatRange(DefaultAttackPower, DefaultAttackPower);
        [Tooltip("근접 콤보 (같은 종류 무기는 같은 에셋 공유)")]
        [SerializeField] private AttackComboData comboData;
        [Tooltip("칼날이 없을 때 몸 기준 판정 구체 중심 (캐릭터 로컬, 맨손 주먹 위치)")]
        [SerializeField] private Vector3 hitOffset = new Vector3(0f, 1f, 1f);
        [Tooltip("몸 기준 판정 구체 반지름 (m)")]
        [SerializeField, Min(0f)] private float hitRadius = 0.8f;

        [Tooltip("칼날 시작점 (무기 모델 로컬, 손잡이 위)")]
        [SerializeField] private Vector3 bladeBase;
        [Tooltip("칼날 끝점 (무기 모델 로컬, 시작점과 같으면 몸 기준 판정)")]
        [SerializeField] private Vector3 bladeTip;
        [Tooltip("칼날 판정 두께 (반지름 m, 0.05~0.15)")]
        [SerializeField, Min(0f)] private float bladeRadius = 0.1f;

        [Tooltip("이 무기를 들었을 때 대기 모션 상태")]
        [SerializeField] private string idleStateName = "Idle_A";

        [NonSerialized] private int idleStateHash;
        [NonSerialized] private bool isHashCached;

        public override ItemCategory Category => ItemCategory.Weapon;
        public override bool CanQuickSlot => true;

        public WeaponType WeaponType => weaponType;
        public EquipHand GripHand => gripHand;
        public bool IsHeldInLeftHand => gripHand == EquipHand.Left;
        public bool OccupiesOffHand => occupiesOffHand || IsHeldInLeftHand;
        public bool AllowsShield => allowsShield;
        public bool IsMagic => weaponType == WeaponType.Wand || weaponType == WeaponType.Staff;
        public bool HasMeleeHit => weaponType == WeaponType.Unarmed || weaponType == WeaponType.OneHanded || weaponType == WeaponType.TwoHanded;
        public override ItemStats BaseStats => ItemStats.Attack(attackPowerRange.Min);
        public override bool HasStat(StatType stat) => stat == StatType.AttackPower;
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

        protected override ItemStats RollStats(float multiplier) => ItemStats.Attack(attackPowerRange.Roll(multiplier));

        protected virtual void OnValidate() => isHashCached = false;
    }
}
