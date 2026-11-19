using ProjectFantasy.Combat;
using UnityEngine;
namespace ProjectFantasy.Flow
{
    // 플레이 기록: 진행 시간(일시정지 제외)·사망 횟수, Begin~End 사이만 집계
    public sealed class PlayRecord : MonoBehaviour
    {
        [SerializeField] private Health playerHealth;
        public float PlayTime { get; private set; }
        public int DeathCount { get; private set; }
        private bool isRecording;
        public void Begin()
        {
            PlayTime = 0;
            DeathCount = 0;
            isRecording = true;
        }
        public void End()
        {
            isRecording = false;
        }
        private void Update()
        {
            if (isRecording) PlayTime += Time.deltaTime;
        }
        private void OnEnable()
        {
            playerHealth.Died += Handle;
        }
        private void OnDisable()
        {
            playerHealth.Died -= Handle;
        }
        private void Handle()
        {
            if (isRecording) DeathCount++;
        }
    }
}
