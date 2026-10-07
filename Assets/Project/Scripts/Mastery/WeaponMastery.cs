using System;
using ProjectFantasy.Items;
using UnityEngine;

namespace ProjectFantasy.Mastery
{
    // 종류별 숙련 레벨·경험치, 티어 장착 가능 판정, 보너스 배율 (플레이어 전용 진행 데이터)
    [DisallowMultipleComponent]
    public sealed class WeaponMastery : MonoBehaviour
    {
        private const int StartLevel = 1;
        private const int NoExperience = 0;
        private const float NoBonus = 1f;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const bool IsTestBuild = true;
#else
        private const bool IsTestBuild = false;
#endif

        [Tooltip("최대 레벨·경험치 곡선·티어 해금 레벨·보너스")]
        [SerializeField] private MasteryData data;

        [Header("Test (에디터·개발 빌드에서만 적용)")]
        [Tooltip("켜면 획득 경험치에 아래 배율을 곱함 (릴리즈 빌드에서는 무시)")]
        [SerializeField] private bool isTestMode;
        [Tooltip("테스트 모드 경험치 배율")]
        [SerializeField, Min(1f)] private float testExperienceMultiplier = 200f;
        [Tooltip("켜면 시작 시 모든 숙련도가 최대 레벨 (릴리즈 빌드에서는 무시)")]
        [SerializeField] private bool startAtMaxLevel;

        private int[] levels = Array.Empty<int>();
        private int[] experiences = Array.Empty<int>();

        public MasteryData Data => data;

        // (종류, 획득량)
        public event Action<MasteryType, int> ExperienceGained;
        // (종류, 새 레벨)
        public event Action<MasteryType, int> LevelChanged;

        private void Awake()
        {
            int count = Enum.GetValues(typeof(MasteryType)).Length;
            levels = new int[count];
            experiences = new int[count];
            int startLevel = IsTestBuild && startAtMaxLevel ? data.MaxLevel : StartLevel;
            for (int i = 0; i < count; i++)
            {
                levels[i] = startLevel;
            }
        }

        // 테스트: 플레이 중 컴포넌트 ⋮ 메뉴에서 실행 (릴리즈 빌드에서는 무시)
        [ContextMenu("테스트: 모든 숙련도 최대")]
        private void SetAllToMaxLevel()
        {
            if (!IsTestBuild || !Application.isPlaying) return;

            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i] >= data.MaxLevel) continue;

                levels[i] = data.MaxLevel;
                experiences[i] = NoExperience;
                LevelChanged?.Invoke((MasteryType)i, levels[i]);
            }
        }

        public int GetLevel(MasteryType type) => levels[(int)type];
        public int GetExperience(MasteryType type) => experiences[(int)type];
        public int GetRequiredExperience(MasteryType type) => data.GetRequiredExperience(GetLevel(type));
        public int GetUnlockedTier(MasteryType type) => data.GetUnlockedTier(GetLevel(type));
        public float GetBonusMultiplier(MasteryType type) => data.GetBonusMultiplier(GetLevel(type));

        // 개체 티어 ≤ 해당 종류 해금 티어 (숙련도가 없는 아이템은 항상 가능)
        public bool CanUse(ItemInstance instance)
        {
            return instance == null || !MasteryMapping.TryGet(instance.Data, out MasteryType type) || instance.Tier <= GetUnlockedTier(type);
        }

        public float GetBonusMultiplier(ItemData item)
        {
            return item != null && MasteryMapping.TryGet(item, out MasteryType type) ? GetBonusMultiplier(type) : NoBonus;
        }

        // 한 번에 여러 레벨 상승 가능, 최대 레벨이면 경험치 누적 안 함
        public void AddExperience(MasteryType type, int amount)
        {
            int index = (int)type;
            amount = ApplyTestMultiplier(amount);
            if (amount <= NoExperience || levels[index] >= data.MaxLevel) return;

            experiences[index] += amount;
            ExperienceGained?.Invoke(type, amount);

            int startLevel = levels[index];
            int required = data.GetRequiredExperience(levels[index]);
            while (levels[index] < data.MaxLevel && experiences[index] >= required)
            {
                experiences[index] -= required;
                levels[index]++;
                required = data.GetRequiredExperience(levels[index]);
            }

            if (levels[index] >= data.MaxLevel) experiences[index] = NoExperience;
            if (levels[index] != startLevel) LevelChanged?.Invoke(type, levels[index]);
        }

        // 릴리즈 빌드에서는 테스트 모드를 켜 둬도 원래 값
        private int ApplyTestMultiplier(int amount)
        {
            return IsTestBuild && isTestMode ? Mathf.RoundToInt(amount * testExperienceMultiplier) : amount;
        }
    }
}
