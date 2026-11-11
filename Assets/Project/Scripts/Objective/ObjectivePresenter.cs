using System.Collections;
using ProjectFantasy.UI;
using UnityEngine;

namespace ProjectFantasy.Objective
{
    // 트래커 알림을 목표 HUD에 반영, 단계 완료 연출 동안 갱신 보류
    public sealed class ObjectivePresenter : MonoBehaviour
    {
        [SerializeField] private ObjectiveTracker objectiveTracker;
        [SerializeField] private ObjectiveView objectiveView;
        private bool directing;
        private Coroutine currentCoroutine;
        private void OnEnable()
        {
            objectiveTracker.Changed += Refresh;
            objectiveTracker.StepCompleted += Directing;
            Refresh();
        }
        private void OnDisable()
        {
            objectiveTracker.Changed -= Refresh;
            objectiveTracker.StepCompleted -= Directing;

            directing = false;
            currentCoroutine = null;
        }
        private void Refresh()
        {
            if (directing) return;

            if (objectiveTracker.IsCompleted)
            {
                objectiveView.ShowCompleted();
                return;
            }

            ObjectiveData objectiveData = objectiveTracker.CurrentObjective;
            objectiveView.Show(objectiveData.Description, objectiveTracker.KillCount, objectiveData.RequiredCount);
        }
        private void Directing(ObjectiveData objectiveData)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(Check(objectiveData));
        }
        IEnumerator Check(ObjectiveData objectiveData)
        {
            directing = true;
            yield return objectiveView.PlayCompleted(objectiveData.Description, objectiveData.RequiredCount);

            directing = false;
            Refresh();
            currentCoroutine = null;
        }
    }
}
