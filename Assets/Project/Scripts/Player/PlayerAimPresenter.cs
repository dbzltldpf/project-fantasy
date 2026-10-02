using ProjectFantasy.CameraSystem;
using ProjectFantasy.UI;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 플레이어 조준 상태를 카메라 숄더뷰와 조준점 UI에 전달 (카메라·UI는 플레이어를 모름)
    [DisallowMultipleComponent]
    public sealed class PlayerAimPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [SerializeField] private CrosshairView crosshair;

        private void OnEnable() => player.AimViewChanged += HandleAimViewChanged;
        private void OnDisable() => player.AimViewChanged -= HandleAimViewChanged;

        // 조준점은 조준 모드에서만 표시
        private void Start() => HandleAimViewChanged(player.IsAimViewActive);

        private void HandleAimViewChanged(bool isAiming)
        {
            thirdPersonCamera.SetAiming(isAiming);
            if (crosshair != null) crosshair.SetVisible(isAiming);
        }
    }
}
