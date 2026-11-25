using System;
using System.Collections.Generic;
using UnityEngine;
namespace ProjectFantasy.Settings
{
    // 화면 모드 선택지 (전체화면·프레임 없는 창·창모드)
    [CreateAssetMenu(fileName = "DisplayModeSettingsData", menuName = "ProjectFantasy/Settings/DisplayMode Settings Data")]
    public sealed class DisplayModeSettingsData : ScriptableObject
    {
        // 선택지 하나: 표시 이름, 화면 모드
        [Serializable]
        public struct DisplayModeOption
        {
            [SerializeField] private string label;
            [SerializeField] private FullScreenMode mode;
            [SerializeField] private int windowWidth;
            [SerializeField] private int windowHeight;
            public string Label => label;
            public int WindowWidth => windowWidth;
            public int WindowHeight => windowHeight;
            public FullScreenMode Mode => mode;
        }
        [SerializeField] private DisplayModeOption[] options;
        [SerializeField] private int defaultIndex = 1;
        public IReadOnlyList<DisplayModeOption> Options => options;
        public int DefaultIndex => defaultIndex;
    }
}
