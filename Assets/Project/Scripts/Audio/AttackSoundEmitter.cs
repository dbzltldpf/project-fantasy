
using ProjectFantasy.Combat;
using ProjectFantasy.Magic;
using UnityEngine;
namespace ProjectFantasy.Audio
{
    // 근접·원거리·마법 이벤트에 맞춰 휘두르기·발사·명중·시전음 재생 (소리는 데이터에서)    [DisallowMultipleComponent]
    public sealed class AttackSoundEmitter : MonoBehaviour
    {
        [SerializeField] private SoundPlayer soundPlayer;
        private RangedAttacker rangedAttacker;
        private SpellCaster spellCaster;
        private MeleeAttacker meleeAttacker;
        private void Awake()
        {
            TryGetComponent(out rangedAttacker);
            TryGetComponent(out spellCaster);
            TryGetComponent(out meleeAttacker);
            if (soundPlayer == null) soundPlayer = FindFirstObjectByType<SoundPlayer>();
        }
        private void OnEnable()
        {
            if (rangedAttacker != null)
            {
                rangedAttacker.ShotFired += HandleShotFired;
                rangedAttacker.Impacted += HandleImpacted;
            }
            if (spellCaster != null)
            {
                spellCaster.CastStarted += HandleCastStarted;
                spellCaster.AreaActivated += HandleAreaActivated;
            }
            if (meleeAttacker != null) meleeAttacker.SoundCued += HandleSoundCued;
        }
        private void OnDisable()
        {
            if (rangedAttacker != null)
            {
                rangedAttacker.ShotFired -= HandleShotFired;
                rangedAttacker.Impacted -= HandleImpacted;
            }
            if (spellCaster != null)
            {
                spellCaster.CastStarted -= HandleCastStarted;
                spellCaster.AreaActivated -= HandleAreaActivated;
            }
            if (meleeAttacker != null) meleeAttacker.SoundCued -= HandleSoundCued;
        }
        private void HandleShotFired(RangedShot rangedShot, ProjectileProfile profile)
        {
            soundPlayer.Play(profile.FireSound, rangedShot.Origin);
        }
        private void HandleImpacted(ProjectileProfile profile, Vector3 position)
        {
            soundPlayer.Play(profile.ImpactSound, position);
        }
        private void HandleCastStarted(SpellData spellData)
        {
            soundPlayer.Play(spellData.CastSound, transform.position);
        }
        private void HandleAreaActivated(AreaSpellData areaSpellData, Vector3 position)
        {
            soundPlayer.Play(areaSpellData.ImpactSound, position);
        }
        private void HandleSoundCued(SoundData soundData)
        {
            soundPlayer.Play(soundData, transform.position);
        }
    }
}
