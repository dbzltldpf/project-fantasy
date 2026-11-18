using ProjectFantasy.World;
using UnityEngine;
namespace ProjectFantasy.Objective
{
    // 장소 도착 목표: 지정한 장소에 한 번 도착
    [CreateAssetMenu(fileName = "ReachObjectiveData", menuName = "ProjectFantasy/Objective/Reach Objective")]
    public sealed class ReachObjectiveData : ObjectiveData
    {
        private const int ReachCount = 1;
        [SerializeField] private LocationData locationData;
        public override int RequiredCount => ReachCount;
        public override bool IsMatch(LocationData locationData) => locationData == this.locationData;
        public override bool ShowProgress => false;
    }

}
