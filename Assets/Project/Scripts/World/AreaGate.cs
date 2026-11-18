using UnityEngine;
namespace ProjectFantasy.World
{
    // 막고 있는 벽을 꺼서 구역을 연다
    public sealed class AreaGate : MonoBehaviour
    {
        [SerializeField] private GameObject blocker;
        private bool isOpen;
        public void Open()
        {
            if (isOpen) return;
            isOpen = true;
            blocker.SetActive(false);
        }
    }

}
