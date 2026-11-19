using System.Collections;
using System.Collections.Generic;
using ProjectFantasy.UI;
using UnityEngine;
namespace ProjectFantasy.Narration
{
    // 내레이션을 한 줄씩 일정 시간 표시 (게임 시간 기준, 일시정지 시 함께 멈춤)
    public sealed class NarrationPlayer : MonoBehaviour
    {
        [SerializeField] private NarrationView narrationView;
        [SerializeField] private float lineDuration = 3f;
        private Coroutine currentCoroutine;
        private WaitForSeconds lineWait;
        private void Awake()
        {
            lineWait = new WaitForSeconds(lineDuration);
        }
        public void Play(NarrationData narrationData)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(PlayLines(narrationData.Lines));
        }
        private IEnumerator PlayLines(IReadOnlyList<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                narrationView.Show(lines[i]);
                yield return lineWait;
            }
            narrationView.Hide();
            currentCoroutine = null;
        }
    }

}
