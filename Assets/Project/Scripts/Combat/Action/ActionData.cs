using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 애니메이션 클립 + 프레임 단위 이벤트 타임라인 (공격 한 동작)
    [CreateAssetMenu(fileName = "ActionData", menuName = "ProjectFantasy/Combat/Action Data")]
    public sealed class ActionData : ScriptableObject
    {
        public const float DefaultFrameRate = 30f;
        private const float MinPlaybackSpeed = 0.01f;

        [Tooltip("기준 클립 (프레임레이트·길이, 타임라인 미리보기)")]
        [SerializeField] private AnimationClip clip;
        [Tooltip("애니메이터 상태 이름 (비우면 클립 이름)")]
        [SerializeField] private string stateName;
        [Tooltip("액션 종료 프레임 (이후 이동 상태로 복귀)")]
        [SerializeField, Min(1)] private int lengthFrames = (int)DefaultFrameRate;
        [Tooltip("애니메이션·이벤트 공통 재생 속도 배율")]
        [SerializeField, Min(MinPlaybackSpeed)] private float playbackSpeed = 1f;
        [Tooltip("이 액션으로 전환하는 블렌드 시간 (초)")]
        [SerializeField, Min(0f)] private float crossFadeDuration = 0.1f;
        [SerializeReference] private List<ActionEvent> events = new List<ActionEvent>();

        [NonSerialized] private int stateHash;
        [NonSerialized] private bool isHashCached;

        public AnimationClip Clip => clip;
        public string StateName => string.IsNullOrEmpty(stateName) && clip != null ? clip.name : stateName ?? string.Empty;
        public float FrameRate => clip != null ? clip.frameRate : DefaultFrameRate;
        public int LengthFrames => lengthFrames;
        public float PlaybackSpeed => playbackSpeed;
        public float CrossFadeDuration => crossFadeDuration;
        public int EventCount => events.Count;

        public int StateHash
        {
            get
            {
                if (!isHashCached)
                {
                    stateHash = Animator.StringToHash(StateName);
                    isHashCached = true;
                }
                return stateHash;
            }
        }

        public ActionEvent GetEvent(int index) => events[index];

        // 이벤트 검증 경고는 인스펙터·타임라인 창에 표시 (드래그 편집 중 콘솔 도배 방지)
        private void OnValidate() => isHashCached = false;
    }
}
