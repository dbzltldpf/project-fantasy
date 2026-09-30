using System;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 무기별 콤보 구성 데이터
    [CreateAssetMenu(fileName = "AttackComboData", menuName = "ProjectFantasy/Combat/Attack Combo Data")]
    public sealed class AttackComboData : ScriptableObject
    {
        [SerializeField] private AttackStep[] steps = Array.Empty<AttackStep>();
        [SerializeField, Min(0f)] private float crossFadeDuration = 0.1f;

        public int StepCount => steps.Length;
        public float CrossFadeDuration => crossFadeDuration;

        public AttackStep GetStep(int index) => steps[index];
        public bool HasNextStep(int index) => index + 1 < steps.Length;

        private void OnValidate()
        {
            foreach (AttackStep step in steps)
            {
                step?.InvalidateCache();
            }
        }
    }
}
