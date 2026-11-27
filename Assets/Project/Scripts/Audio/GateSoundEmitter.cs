using ProjectFantasy.World;
using UnityEngine;
namespace ProjectFantasy.Audio
{
    // 구역 게이트 열림·닫힘 소리 (2D, 진행 알림)
    [RequireComponent(typeof(AreaGate))]
    [DisallowMultipleComponent]
    public sealed class GateSoundEmitter : MonoBehaviour
    {
        [SerializeField] private SoundPlayer soundPlayer;
        [SerializeField] private SoundData openSound;
        [SerializeField] private SoundData closeSound;
        private AreaGate areaGate;
        private void Awake()
        {
            areaGate = GetComponent<AreaGate>();
            if (soundPlayer == null) soundPlayer = FindFirstObjectByType<SoundPlayer>();
        }
        private void OnEnable()
        {
            areaGate.Opened += HandleOpened;
            areaGate.Closed += HandleClosed;
        }
        private void OnDisable()
        {
            areaGate.Opened -= HandleOpened;
            areaGate.Closed -= HandleClosed;
        }
        private void HandleOpened()
        {
            soundPlayer.Play(openSound, transform.position);
        }
        private void HandleClosed()
        {
            soundPlayer.Play(closeSound, transform.position);
        }
    }

}
