using System.Collections.Generic;
using System.Linq;
using ProjectFantasy.UI;
using UnityEngine;
namespace ProjectFantasy.Settings
{
    // 화면 모드 적용·저장(PlayerPrefs), 시작 시 저장값 복원
    public sealed class DisplayModeSettings : MonoBehaviour
    {
        private const string PrefsKey = "Settings.DisplayMode";
        [SerializeField] private SettingsDropdownView dropdownView;
        [SerializeField] private DisplayModeSettingsData settingsData;
        public int CurrentIndex { get; private set; }
        private void Awake()
        {
            int saved = PlayerPrefs.GetInt(PrefsKey, settingsData.DefaultIndex);
            if (saved < 0 || saved >= settingsData.Options.Count) saved = settingsData.DefaultIndex;
            Apply(saved);
        }
        private void Start()
        {
            List<string> labels = settingsData.Options.Select(x => x.Label).ToList();
            dropdownView.SetOptions(labels, CurrentIndex);
        }
        private void OnEnable()
        {
            dropdownView.Selected += Select;
        }
        private void OnDisable()
        {
            dropdownView.Selected -= Select;
        }

        private void Select(int index)
        {
            Apply(index);
            PlayerPrefs.SetInt(PrefsKey, index);
            PlayerPrefs.Save();
        }
        private void Apply(int index)
        {
            CurrentIndex = index;
            DisplayModeSettingsData.DisplayModeOption option = settingsData.Options[index];
            if (option.Mode == FullScreenMode.Windowed)
            {
                Screen.SetResolution(option.WindowWidth, option.WindowHeight, option.Mode);
                return;
            }
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, option.Mode);
        }
    }
}
