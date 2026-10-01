using UnityEngine;

namespace ProjectFantasy.Weapon
{
    // 손에 드는 장비 공통 데이터 (모델, 손 위치 보정)
    public abstract class EquipmentData : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private GameObject modelPrefab;
        [SerializeField] private Vector3 gripPosition;
        [SerializeField] private Vector3 gripRotation;

        public string DisplayName => displayName;
        public GameObject ModelPrefab => modelPrefab;
        public Vector3 GripPosition => gripPosition;
        public Quaternion GripRotation => Quaternion.Euler(gripRotation);
    }
}
