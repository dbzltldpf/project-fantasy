using UnityEngine;

namespace ProjectFantasy.Weapon
{
    // 활/석궁: 화살 종류, 조준·장전 여부, 발사/장전 모션과 타이밍 (시간 단위: 초)
    [CreateAssetMenu(fileName = "RangedWeaponData", menuName = "ProjectFantasy/Weapon/Ranged Weapon Data")]
    public sealed class RangedWeaponData : WeaponData
    {
        [Header("Ranged")]
        [SerializeField] private AmmoData ammo;
        [Tooltip("우클릭 조준 모드 사용 (한손 석궁은 false)")]
        [SerializeField] private bool canAim = true;
        [Tooltip("발사 후 장전 필요 (석궁)")]
        [SerializeField] private bool requiresReload;
        [Tooltip("캐릭터 로컬 기준 발사 지점 (조준 모드 경로선 시작점으로 확인)")]
        [SerializeField] private Vector3 muzzleOffset = new Vector3(0f, 1.4f, 0.5f);
        [SerializeField, Min(0f)] private float launchSpeed = 40f;

        [Header("Ranged Animation")]
        [Tooltip("비조준 발사 시 먼저 재생 (활 당기기), 비우면 생략")]
        [SerializeField] private string windupStateName = "Ranged_Bow_Draw";
        [SerializeField, Min(0f)] private float windupDuration = 0.5f;
        [SerializeField] private string fireStateName = "Ranged_Bow_Release";
        [Tooltip("발사 모션 시작 후 화살이 나가는 시점")]
        [SerializeField, Min(0f)] private float releaseTime = 0.05f;
        [SerializeField, Min(0f)] private float fireDuration = 0.5f;
        [SerializeField] private string aimIdleStateName = "Ranged_Bow_Aiming_Idle";
        [Tooltip("조준 대기 자세를 멈출 정규화 시간 (1이면 고정 안 함)")]
        [SerializeField, Range(0f, 1f)] private float aimHoldTime = 1f;
        [SerializeField] private string reloadStateName;
        [SerializeField, Min(0f)] private float reloadDuration = 1f;

        [Header("Loaded Ammo Visual")]
        [Tooltip("장전된 볼트의 무기 모델 기준 위치/회전 (석궁)")]
        [SerializeField] private Vector3 loadedAmmoPosition;
        [SerializeField] private Vector3 loadedAmmoRotation;

        private int windupStateHash;
        private int fireStateHash;
        private int aimIdleStateHash;
        private int reloadStateHash;

        public AmmoData Ammo => ammo;
        public bool CanAim => canAim;
        public bool RequiresReload => requiresReload;
        public bool ShowsNockedAmmo => !requiresReload;
        public Vector3 MuzzleOffset => muzzleOffset;
        public float LaunchSpeed => launchSpeed;

        public bool HasWindup => !string.IsNullOrEmpty(windupStateName);
        public string WindupStateName => windupStateName;
        public int WindupStateHash => windupStateHash;
        public float WindupDuration => windupDuration;
        public string FireStateName => fireStateName;
        public int FireStateHash => fireStateHash;
        public float ReleaseTime => releaseTime;
        public float FireDuration => fireDuration;
        public string AimIdleStateName => aimIdleStateName;
        public int AimIdleStateHash => aimIdleStateHash;
        public float AimHoldTime => aimHoldTime;
        public string ReloadStateName => reloadStateName;
        public int ReloadStateHash => reloadStateHash;
        public float ReloadDuration => reloadDuration;
        public Vector3 LoadedAmmoPosition => loadedAmmoPosition;
        public Quaternion LoadedAmmoRotation => Quaternion.Euler(loadedAmmoRotation);

        private void OnEnable() => CacheStateHashes();

        protected override void OnValidate()
        {
            base.OnValidate();
            CacheStateHashes();
        }

        private void CacheStateHashes()
        {
            windupStateHash = Animator.StringToHash(windupStateName);
            fireStateHash = Animator.StringToHash(fireStateName);
            aimIdleStateHash = Animator.StringToHash(aimIdleStateName);
            reloadStateHash = Animator.StringToHash(reloadStateName);
        }
    }
}
