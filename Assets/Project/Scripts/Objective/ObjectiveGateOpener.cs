using ProjectFantasy.World;
using UnityEngine;
namespace ProjectFantasy.Objective
{
    // 지정한 목표가 끝나면 구역 개방
    public sealed class ObjectiveGateOpener : MonoBehaviour
    {
        [SerializeField] private ObjectiveTracker objectiveTracker;
        [SerializeField] private ObjectiveData targetObjective;
        [SerializeField] private AreaGate areaGate;
        private void OnEnable()
        {
            objectiveTracker.StepCompleted += HandleStepCompleted;
        }
        private void OnDisable()
        {
            objectiveTracker.StepCompleted -= HandleStepCompleted;
        }
        private void HandleStepCompleted(ObjectiveData objectiveData)
        {
            if (objectiveData != targetObjective) return;
            areaGate.Open();
        }
    }

}
