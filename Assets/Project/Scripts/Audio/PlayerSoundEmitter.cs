using ProjectFantasy.Player;
using UnityEngine;
namespace ProjectFantasy.Audio
{
    // 플레이어 행동 소리 (활 당기기·장전 완료·장비 교체·줍기)
    [RequireComponent(typeof(PlayerController))]
    [DisallowMultipleComponent]
    public sealed class PlayerSoundEmitter : MonoBehaviour
    {
        [SerializeField] private SoundPlayer soundPlayer;
        [SerializeField] private SoundData drawSound;
        [SerializeField] private SoundData reloadSound;
        [SerializeField] private SoundData equipSound;
        [SerializeField] private SoundData pickUpSound;
        private PlayerController controller;
        private PlayerRangedWeapon rangedWeapon;
        private PlayerItemHandler itemHandler;
        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            rangedWeapon = GetComponent<PlayerRangedWeapon>();
            itemHandler = GetComponent<PlayerItemHandler>();
            if (soundPlayer == null) soundPlayer = FindFirstObjectByType<SoundPlayer>();
        }
        private void OnEnable()
        {
            controller.Draw.Started += HandleDrawStarted;
            rangedWeapon.LoadedChanged += HandleReloadChanged;
            itemHandler.Equipped += HandleEquipped;
            itemHandler.PickedUp += HandlePickUp;
        }
        private void OnDisable()
        {
            controller.Draw.Started -= HandleDrawStarted;
            rangedWeapon.LoadedChanged -= HandleReloadChanged;
            itemHandler.Equipped -= HandleEquipped;
            itemHandler.PickedUp -= HandlePickUp;
        }
        private void HandleDrawStarted()
        {
            soundPlayer.Play(drawSound, transform.position);
        }
        private void HandleReloadChanged(bool isLoaded)
        {
            if (isLoaded) soundPlayer.Play(reloadSound, transform.position);
        }
        private void HandleEquipped()
        {
            soundPlayer.Play(equipSound, transform.position);
        }
        private void HandlePickUp()
        {
            soundPlayer.Play(pickUpSound, transform.position);
        }

    }

}
