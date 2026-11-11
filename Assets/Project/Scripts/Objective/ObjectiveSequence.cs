using System.Collections.Generic;
using UnityEngine;
namespace ProjectFantasy.Objective
{
    // 챕터 하나의 목표 단계 목록 (진행 순서대로)
    [CreateAssetMenu(fileName = "ObjectiveSequence", menuName = "ProjectFantasy/Objective/Objective Sequence")]
    public sealed class ObjectiveSequence : ScriptableObject
    {
        [SerializeField] private ObjectiveData[] objectiveDatas;
        public IReadOnlyList<ObjectiveData> Objectives => objectiveDatas;
    }

}
