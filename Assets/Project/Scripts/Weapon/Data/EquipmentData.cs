using ProjectFantasy.Items;
using UnityEngine;

namespace ProjectFantasy.Weapon
{
    // 손에 드는 장비 공통 데이터 (모델, 손 위치 보정, 등급표), 필드 모델 기본값은 장비 모델
    public abstract class EquipmentData : ItemData
    {
        private const float NoGradeMultiplier = 1f;

        [Tooltip("손에 쥐는 모델 (맨손은 비움)")]
        [SerializeField] private GameObject modelPrefab;
        [Tooltip("손 소켓 기준 위치 보정 (KayKit 무기는 0 유지)")]
        [SerializeField] private Vector3 gripPosition;
        [Tooltip("손 소켓 기준 회전 보정 (KayKit 무기는 0 유지)")]
        [SerializeField] private Vector3 gripRotation;
        [Tooltip("등급 추첨 표 (비우면 등급 없이 범위 값만 굴림)")]
        [SerializeField] private ItemGradeTable gradeTable;

        public GameObject ModelPrefab => modelPrefab;
        public Vector3 GripPosition => gripPosition;
        public Quaternion GripRotation => Quaternion.Euler(gripRotation);

        public override GameObject WorldModel => base.WorldModel != null ? base.WorldModel : modelPrefab;

        // 개체 없이 쓰는 기본 능력치 (맨손 등, 범위 최솟값)
        public virtual ItemStats BaseStats => ItemStats.Zero;

        public override ItemInstance CreateInstance()
        {
            ItemGrade grade = gradeTable != null ? gradeTable.Roll() : null;
            float multiplier = grade != null ? grade.StatMultiplier : NoGradeMultiplier;
            return new ItemInstance(this, grade, RollStats(multiplier));
        }

        protected virtual ItemStats RollStats(float multiplier) => ItemStats.Zero;
    }
}
