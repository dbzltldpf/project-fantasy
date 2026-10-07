using System;
using UnityEngine;

namespace ProjectFantasy.Mastery
{
    // 숙련도 규칙: 최대 레벨, 경험치 곡선, 티어 해금 레벨, 레벨당 보너스, 종류별 이름
    [CreateAssetMenu(fileName = "MasteryData", menuName = "ProjectFantasy/Mastery/Mastery Data")]
    public sealed class MasteryData : ScriptableObject
    {
        private const int MinLevel = 1;
        private const int FirstTier = 1;
        private const float NoBonus = 1f;
        private const int NoExperience = 0;

        [Tooltip("최대 숙련 레벨")]
        [SerializeField, Min(MinLevel)] private int maxLevel = 50;
        [Tooltip("다음 레벨 필요 경험치 = 기본값 × 현재 레벨^지수")]
        [SerializeField, Min(1)] private int experienceBase = 100;
        [Tooltip("경험치 곡선 지수 (1이면 직선, 클수록 고레벨이 오래 걸림)")]
        [SerializeField, Min(1f)] private float experienceExponent = 1.5f;
        [Tooltip("티어별 해금 레벨 (순서대로 T1, T2 …)")]
        [SerializeField] private int[] tierUnlockLevels = { 1, 10, 20, 30, 40 };
        [Tooltip("레벨당 보너스 (무기·마법서 = 피해, 방패 = 방어력), 0.01 = 1%")]
        [SerializeField, Min(0f)] private float bonusPerLevel = 0.01f;
        [Tooltip("숙련도 창·알림 이름 (MasteryType 순서)")]
        [SerializeField] private string[] displayNames = { "맨손", "한손검", "양손검", "활", "한손 석궁", "양손 석궁", "Wand", "Staff", "방패", "마법서" };

        public int MaxLevel => maxLevel;
        public int MaxTier => tierUnlockLevels.Length;

        // 현재 레벨에서 다음 레벨까지 필요 경험치 (최대 레벨이면 0)
        public int GetRequiredExperience(int level)
        {
            if (level >= maxLevel) return NoExperience;
            return Mathf.RoundToInt(experienceBase * Mathf.Pow(level, experienceExponent));
        }

        // 레벨로 해금된 최고 티어
        public int GetUnlockedTier(int level)
        {
            int tier = FirstTier;
            for (int i = 0; i < tierUnlockLevels.Length; i++)
            {
                if (level >= tierUnlockLevels[i]) tier = i + FirstTier;
            }
            return tier;
        }

        // 티어 해금에 필요한 레벨 (정의 밖 티어는 최대 레벨 초과로 사실상 불가)
        public int GetRequiredLevel(int tier)
        {
            int index = tier - FirstTier;
            return index >= 0 && index < tierUnlockLevels.Length ? tierUnlockLevels[index] : maxLevel + MinLevel;
        }

        public float GetBonusMultiplier(int level) => NoBonus + (level - MinLevel) * bonusPerLevel;

        public string GetDisplayName(MasteryType type)
        {
            int index = (int)type;
            return index < displayNames.Length && !string.IsNullOrEmpty(displayNames[index]) ? displayNames[index] : type.ToString();
        }

        private void OnValidate()
        {
            if (displayNames.Length != Enum.GetValues(typeof(MasteryType)).Length)
            {
                Debug.LogWarning($"[{name}] displayNames 개수가 MasteryType 개수와 다릅니다.", this);
            }
        }
    }
}
