using System;
using System.Collections.Generic;
using UnityEngine;
namespace ProjectFantasy.Settings
{
    // 프레임 제한 선택지 (VSync 또는 목표 프레임)
    [CreateAssetMenu(fileName = "FrameRateSettingsData", menuName = "ProjectFantasy/Settings/Frame Rate Settings Data")]
    public sealed class FrameRateSettingsData : ScriptableObject
    {
        // 선택지 하나: 표시 이름, VSync 사용 여부, 목표 프레임(-1 = 제한 없음)
        [Serializable]
        public struct FrameRateOption
        {
            [Tooltip("드롭다운에 표시할 이름")]
            [SerializeField] private string label;
            [Tooltip("켜면 모니터 주사율에 맞춤 (목표 프레임 무시)")]
            [SerializeField] private bool useVSync;
            [Tooltip("VSync를 끈 경우 목표 프레임 (-1 = 제한 없음)")]
            [SerializeField] private int targetFrameRate;
            public string Label => label;
            public bool UseVSync => useVSync;
            public int TargetFrameRate => targetFrameRate;
        }
        [Tooltip("드롭다운 순서대로 선택지")]
        [SerializeField] private FrameRateOption[] options;
        public IReadOnlyList<FrameRateOption> Options => options;
        [Tooltip("저장값이 없을 때 선택지 (0 = 모니터 주사율)")]
        [SerializeField] private int defaultIndex = 0;
        public int DefaultIndex => defaultIndex;
    }

}
