using ProjectFantasy.CameraSystem;
using ProjectFantasy.UI;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 플레이어 조준 상태를 카메라 숄더뷰와 조준점 UI에 전달 (카메라·UI는 플레이어를 모름)
    [DisallowMultipleComponent]
    public sealed class PlayerAimPresenter : MonoBehaviour
    {
        private const float EmptyRatio = 0f;
        [Tooltip("플레이어")]
        [SerializeField] private PlayerController player;
        [Tooltip("3인칭 카메라")]
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [Tooltip("조준점 UI")]
        [SerializeField] private CrosshairView crosshair;
        private bool isAiming;
        private void OnEnable() => player.AimViewChanged += HandleAimViewChanged;
        private void OnDisable() => player.AimViewChanged -= HandleAimViewChanged;

        // 평소엔 점, 조준 모드엔 조준점
        private void Start() => HandleAimViewChanged(player.IsAimViewActive);

        private void HandleAimViewChanged(bool isAiming)
        {
            this.isAiming = isAiming;
            thirdPersonCamera.SetAiming(isAiming);
            if (crosshair != null) crosshair.SetAiming(isAiming);
            if (!isAiming && crosshair != null) crosshair.SetDraw(false, EmptyRatio);
        }
        // 조준 중에만 당김 게이지 갱신
        private void LateUpdate()
        {
            if (!isAiming || crosshair == null) return;
            crosshair.SetDraw(player.Draw.IsDrawing, player.Draw.Ratio);
        }
    }
}
