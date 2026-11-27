using System;
using ProjectFantasy.Combat;
using UnityEngine;
namespace ProjectFantasy.Audio
{
    // 지정 프레임에 소리 신호 (휘두르기·기합 등)
    [Serializable]
    public sealed class SoundEvent : ActionEvent
    {
        [Tooltip("재생할 소리 (휘두르기는 타격 구간 1~2프레임 전)")]
        [SerializeField] private SoundData sound;

        public override void OnBegin(IActionContext context) => context.PlaySound(sound);

        public override string Validate(int lengthFrames) => sound == null ? "소리(Sound)가 비어 있습니다." : base.Validate(lengthFrames);
    }
}
