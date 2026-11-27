
using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using ProjectFantasy.Weapon;
using UnityEngine;
namespace ProjectFantasy.Audio
{
    // 체력 이벤트에 맞춰 피격·막기·가드 파괴·사망음 재생 (플레이어·적·허수아비 공용)
    [RequireComponent(typeof(Health))]
    [DisallowMultipleComponent]
    public sealed class HealthSoundEmitter : MonoBehaviour
    {
        private const int NoFrame = -1;
        private int guardBrokenFrame = NoFrame;
        [SerializeField] private SoundPlayer soundPlayer;
        [SerializeField] private SoundData hitSound;
        [SerializeField] private SoundData heavyHitSound;
        [SerializeField] private SoundData blockSound;
        [SerializeField] private SoundData guardBreakSound;
        [SerializeField] private SoundData deathSound;
        private Health health;
        private ShieldGuard shieldGuard;
        private void Awake()
        {
            health = GetComponent<Health>();
            TryGetComponent(out shieldGuard);
            if (soundPlayer == null) soundPlayer = FindFirstObjectByType<SoundPlayer>();
        }
        private void OnEnable()
        {
            health.Damaged += HandleDamaged;
            health.Blocked += HandleBlocked;
            health.Died += HandleDied;
            if (shieldGuard != null) shieldGuard.GuardBroken += HandleGuardBroken;
        }
        private void OnDisable()
        {
            health.Damaged -= HandleDamaged;
            health.Blocked -= HandleBlocked;
            health.Died -= HandleDied;
            if (shieldGuard != null) shieldGuard.GuardBroken -= HandleGuardBroken;
        }
        // 물리만 재생 (마법은 투사체 명중음이 담당), 양손검이면 무거운 타격음
        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (Time.frameCount == guardBrokenFrame) return;
            if (damageInfo.Type != DamageType.Physical) return;
            bool isHeavy = heavyHitSound != null && damageInfo.IsFromWeapon(WeaponType.TwoHanded);
            soundPlayer.Play(isHeavy ? heavyHitSound : hitSound, damageInfo.HitPoint);
        }
        private void HandleBlocked(DamageInfo damageInfo) => soundPlayer.Play(blockSound, damageInfo.HitPoint);
        private void HandleGuardBroken()
        {
            guardBrokenFrame = Time.frameCount;
            soundPlayer.Play(guardBreakSound, transform.position);
        }
        private void HandleDied() => soundPlayer.Play(deathSound, transform.position);
    }
}
