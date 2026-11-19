using UnityEngine;
namespace ProjectFantasy.World
{
    // 막는 벽을 끄고 켜서 구역을 열고 닫음
    public sealed class AreaGate : MonoBehaviour
    {
        [SerializeField] private GameObject blocker;
        [Tooltip("열렸을 때 보이는 통과 가능한 문 (콜라이더 없음)")]
        [SerializeField] private GameObject openVisual;

        private bool isOpen;
        public bool IsOpen => isOpen;
        private void Awake()
        {
            SetOpen(false);
        }
        private void SetOpen(bool open)
        {
            isOpen = open;
            blocker.SetActive(!open);
            openVisual.SetActive(open);
        }
        public void Open()
        {
            if (isOpen) return;
            SetOpen(true);
        }
        public void Close()
        {
            if (!isOpen) return;
            SetOpen(false);
        }
    }

}
