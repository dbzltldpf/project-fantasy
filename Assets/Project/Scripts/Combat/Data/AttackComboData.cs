using System;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 무기별 콤보 구성 (단계마다 ActionData 하나)
    [CreateAssetMenu(fileName = "AttackComboData", menuName = "ProjectFantasy/Combat/Attack Combo Data")]
    public sealed class AttackComboData : ScriptableObject
    {
        [SerializeField] private ActionData[] actions = Array.Empty<ActionData>();

        public int ActionCount => actions.Length;

        public ActionData GetAction(int index) => actions[index];
        public bool HasNextAction(int index) => index + 1 < actions.Length;

        private void OnValidate()
        {
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] == null) Debug.LogWarning($"[{name}] Action {i}이 비어 있습니다.", this);
            }
        }
    }
}
