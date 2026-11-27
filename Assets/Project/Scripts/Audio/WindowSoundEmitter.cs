using ProjectFantasy.UI;
using UnityEngine;
namespace ProjectFantasy.Audio
{
    // 인벤토리 창 열기·닫기 소리 (2D)
    [RequireComponent(typeof(InventoryWindow))]
    [DisallowMultipleComponent]
    public sealed class WindowSoundEmitter : MonoBehaviour
    {
        [SerializeField] private SoundPlayer soundPlayer;
        [SerializeField] private SoundData openSound;
        [SerializeField] private SoundData closeSound;
        private InventoryWindow inventoryWindow;
        private void Awake()
        {
            inventoryWindow = GetComponent<InventoryWindow>();
            if (soundPlayer == null) soundPlayer = FindFirstObjectByType<SoundPlayer>();
        }
        private void OnEnable()
        {
            inventoryWindow.OpenChanged += HandleOpenClosed;
        }
        private void OnDisable()
        {
            inventoryWindow.OpenChanged -= HandleOpenClosed;
        }
        private void HandleOpenClosed(bool isOpen)
        {
            SoundData soundData = isOpen ? openSound : closeSound;
            soundPlayer.Play(soundData, transform.position);
        }
    }

}
