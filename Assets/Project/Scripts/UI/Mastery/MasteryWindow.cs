using System;
using System.Collections.Generic;
using ProjectFantasy.Mastery;
using UnityEngine;

namespace ProjectFantasy.UI
{
    // 숙련도 창 (K, 실시간): 종류별 레벨·해금 티어·경험치·보너스, 경험치·레벨 변경 시 해당 줄만 갱신
    [DisallowMultipleComponent]
    public sealed class MasteryWindow : MonoBehaviour
    {
        [Tooltip("열고 닫는 패널 (컴포넌트는 항상 활성인 바깥 오브젝트에 둘 것)")]
        [SerializeField] private GameObject root;
        [Tooltip("줄이 생성될 부모 (Vertical Layout)")]
        [SerializeField] private RectTransform rowContainer;
        [SerializeField] private MasteryRowView rowPrefab;

        private readonly List<MasteryRowView> rows = new List<MasteryRowView>();
        private WeaponMastery mastery;

        public bool IsOpen => root.activeSelf;

        public event Action<bool> OpenChanged;

        private void Awake() => root.SetActive(false);

        private void OnDestroy()
        {
            if (mastery == null) return;

            mastery.ExperienceGained -= HandleMasteryChanged;
            mastery.LevelChanged -= HandleMasteryChanged;
        }

        // 플레이어 쪽 Presenter가 시작 시 1회 연결
        public void Bind(WeaponMastery targetMastery)
        {
            mastery = targetMastery;
            mastery.ExperienceGained += HandleMasteryChanged;
            mastery.LevelChanged += HandleMasteryChanged;

            foreach (MasteryType type in (MasteryType[])Enum.GetValues(typeof(MasteryType)))
            {
                MasteryRowView row = Instantiate(rowPrefab, rowContainer);
                row.SetName(mastery.Data.GetDisplayName(type));
                rows.Add(row);
                RefreshRow(type);
            }
        }

        public void Toggle() => SetOpen(!IsOpen);

        public void SetOpen(bool isOpen)
        {
            if (IsOpen == isOpen) return;

            root.SetActive(isOpen);
            OpenChanged?.Invoke(isOpen);
        }

        private void RefreshRow(MasteryType type)
        {
            rows[(int)type].SetProgress(mastery.GetLevel(type), mastery.GetUnlockedTier(type), mastery.GetExperience(type),
                mastery.GetRequiredExperience(type), mastery.GetBonusMultiplier(type));
        }

        private void HandleMasteryChanged(MasteryType type, int _) => RefreshRow(type);
    }
}
