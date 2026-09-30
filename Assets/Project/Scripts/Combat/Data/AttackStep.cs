using System;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 콤보 한 단계의 타이밍/데미지 (시간 단위: 초)
    [Serializable]
    public sealed class AttackStep
    {
        [SerializeField] private string stateName = "Melee_1H_Attack_Slice_Horizontal";
        [SerializeField, Min(0)] private int damage = 10;
        [SerializeField, Min(0f)] private float duration = 0.7f;
        [SerializeField, Min(0f)] private float hitStartTime = 0.2f;
        [SerializeField, Min(0f)] private float hitEndTime = 0.35f;
        [SerializeField, Min(0f)] private float comboInputTime = 0.3f;
        [SerializeField, Min(0f)] private float lungeSpeed = 3f;
        [SerializeField, Min(0f)] private float lungeDuration = 0.15f;

        [NonSerialized] private int stateHash;
        [NonSerialized] private bool isHashCached;

        public string StateName => stateName;
        public int Damage => damage;
        public float LungeSpeed => lungeSpeed;

        public int StateHash
        {
            get
            {
                if (!isHashCached)
                {
                    stateHash = Animator.StringToHash(stateName);
                    isHashCached = true;
                }
                return stateHash;
            }
        }

        public bool IsInHitWindow(float elapsedTime) => elapsedTime >= hitStartTime && elapsedTime <= hitEndTime;
        public bool CanChainCombo(float elapsedTime) => elapsedTime >= comboInputTime;
        public bool IsLunging(float elapsedTime) => elapsedTime < lungeDuration;
        public bool IsFinished(float elapsedTime) => elapsedTime >= duration;

        public void InvalidateCache() => isHashCached = false;
    }
}
