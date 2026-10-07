using System;
using ProjectFantasy.Combat;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 방패 적의 막기 행동 (주기적 막기, 피격 후 반응 막기, 막은 뒤 반격)
    [Serializable]
    public sealed class EnemyGuardSettings
    {
        [Tooltip("공격 대신 막기를 선택할 확률 (공격 거리·쿨타임 준비 시 판정)")]
        [SerializeField, Range(0f, 1f)] private float guardChance = 0.35f;
        [Tooltip("피격 직후 막기로 전환할 확률 (경직 후 판정)")]
        [SerializeField, Range(0f, 1f)] private float reactiveGuardChance = 0.5f;
        [Tooltip("막기 유지 시간 (초)")]
        [SerializeField, Min(0f)] private float guardDuration = 1.5f;
        [Tooltip("막기 후 다시 막기까지 대기 시간 (초)")]
        [SerializeField, Min(0f)] private float guardCooldown = 4f;
        [Tooltip("막기 중 이동 속도 (m/s, 대상을 바라보며 접근)")]
        [SerializeField, Min(0f)] private float guardMoveSpeed = 0.8f;
        [Tooltip("공격을 막은 직후 반격할 확률")]
        [SerializeField, Range(0f, 1f)] private float counterChance = 0.5f;
        [Tooltip("반격 액션 (예: Melee_Block_Attack, 비우면 반격 안 함)")]
        [SerializeField] private ActionData counterAction;

        public float GuardChance => guardChance;
        public float ReactiveGuardChance => reactiveGuardChance;
        public float GuardDuration => guardDuration;
        public float GuardCooldown => guardCooldown;
        public float GuardMoveSpeed => guardMoveSpeed;
        public float CounterChance => counterChance;
        public ActionData CounterAction => counterAction;
    }
}
