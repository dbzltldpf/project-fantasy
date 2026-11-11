using System.Collections.Generic;
using UnityEngine;
namespace ProjectFantasy.Objective
{
    [CreateAssetMenu(fileName = "ObjectiveSequence", menuName = "ProjectFantasy/Objective/Objective Sequence")]
    public sealed class ObjectiveSequence : ScriptableObject
    {
        [SerializeField] private ObjectiveData[] objectiveDatas;
        public IReadOnlyList<ObjectiveData> Objectives => objectiveDatas;
    }

}
