using System;
using ProjectFantasy.Magic;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 시전 마법 한 줄: 마법과 선택 가중치 (쿨타임이 끝난 마법끼리 비교)
    [Serializable]
    public sealed class CasterSpell
    {
        [SerializeField] private SpellData spell;
        [Tooltip("선택 가중치 (클수록 자주, 쿨타임이 끝난 마법끼리 비교)")]
        [SerializeField, Min(0f)] private float weight = 1f;

        public SpellData Spell => spell;
        public float Weight => weight;
    }
}
