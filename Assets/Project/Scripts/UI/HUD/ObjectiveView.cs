using System.Collections;
using TMPro;
using UnityEngine;
namespace ProjectFantasy.UI
{
    // 목표 문구·진행도 표시와 단계 완료 색 연출 (목표 시스템은 모름)
    public sealed class ObjectiveView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_Text progressLabel;
        [SerializeField] private string completed;
        private float completedDisplayDuration = 1.5f;
        private WaitForSeconds waitForSeconds;

        private Color completedColor = Color.gold;
        private Color baseColor;
        private void Awake()
        {
            waitForSeconds = new WaitForSeconds(completedDisplayDuration);
            baseColor = label.color;
        }
        public void Show(string description, int current, int required)
        {
            ResetLabelColor();
            label.text = description;
            progressLabel.enabled = true;
            progressLabel.SetText("{0} / {1}", current, required);
        }
        public void ShowCompleted()
        {
            ResetLabelColor();
            progressLabel.enabled = false;
            label.text = completed;
        }
        public IEnumerator PlayCompleted(string description, int required)
        {
            ResetLabelColor();
            label.SetText($"{description} {completed}");
            progressLabel.SetText("{0} / {1}", required, required);
            label.color = completedColor;
            progressLabel.color = completedColor;

            yield return waitForSeconds;
            ResetLabelColor();
        }
        private void ResetLabelColor()
        {
            label.color = baseColor;
            progressLabel.color = baseColor;
        }
    }

}
