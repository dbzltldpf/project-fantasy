using System.Collections.Generic;
using ProjectFantasy.Utils;
using UnityEngine;

namespace ProjectFantasy.Weapon
{
    // 손 본에 장비 모델 부착 (미리 생성 후 활성/비활성만 전환)
    [DisallowMultipleComponent]
    public sealed class EquipmentVisual : MonoBehaviour
    {
        private const string RightHandSlotName = "handslot.r";
        private const string LeftHandSlotName = "handslot.l";
        private const int HandCount = 2;

        [SerializeField] private Transform rightHandSlot;
        [SerializeField] private Transform leftHandSlot;

        private readonly Dictionary<EquipmentData, GameObject> modelCache = new Dictionary<EquipmentData, GameObject>();
        private readonly GameObject[] activeModels = new GameObject[HandCount];

        private void Reset() => FindHandSlots();

        private void Awake()
        {
            if (rightHandSlot == null || leftHandSlot == null) FindHandSlots();
        }

        public void Preload(EquipmentData data, EquipHand hand) => GetOrCreateModel(data, GetSlot(hand));

        // data가 null이면 해당 손을 비움
        public void Show(EquipHand hand, EquipmentData data)
        {
            int index = (int)hand;
            if (activeModels[index] != null) activeModels[index].SetActive(false);

            activeModels[index] = GetOrCreateModel(data, GetSlot(hand));
            if (activeModels[index] != null) activeModels[index].SetActive(true);
        }

        public void Hide(EquipHand hand) => Show(hand, null);

        public Transform GetActiveModel(EquipHand hand)
        {
            GameObject model = activeModels[(int)hand];
            return model != null ? model.transform : null;
        }

        private Transform GetSlot(EquipHand hand) => hand == EquipHand.Left ? leftHandSlot : rightHandSlot;

        private GameObject GetOrCreateModel(EquipmentData data, Transform slot)
        {
            if (data == null || data.ModelPrefab == null) return null;
            if (modelCache.TryGetValue(data, out GameObject model)) return model;

            model = Instantiate(data.ModelPrefab, slot);
            model.transform.SetLocalPositionAndRotation(data.GripPosition, data.GripRotation);
            model.SetActive(false);
            modelCache.Add(data, model);
            return model;
        }

        private void FindHandSlots()
        {
            if (rightHandSlot == null) rightHandSlot = transform.FindDeepChild(RightHandSlotName);
            if (leftHandSlot == null) leftHandSlot = transform.FindDeepChild(LeftHandSlotName);
        }
    }
}
