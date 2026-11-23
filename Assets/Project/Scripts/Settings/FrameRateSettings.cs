using System.Collections.Generic;
using System.Linq;
using ProjectFantasy.UI;
using UnityEngine;
namespace ProjectFantasy.Settings
{
    // 프레임 제한 적용·저장(PlayerPrefs), 시작 시 저장값 복원
    public sealed class FrameRateSettings : MonoBehaviour
    {
        private const string PrefsKey = "Settings.FrameRate";
        private const int VSyncOff = 0;
        private const int VSyncEveryFrame = 1;
        private const int UnlimitedFrameRate = -1;
        [SerializeField] private FrameRateDropdownView dropdownView;
        [SerializeField] private FrameRateSettingsData settingsData;
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
        public void Select(int index)
        {
            Apply(index);
            PlayerPrefs.SetInt(PrefsKey, index);
            PlayerPrefs.Save();
        }
        private void Apply(int index)
        {
            CurrentIndex = index;
            FrameRateSettingsData.FrameRateOption option = settingsData.Options[index];
            QualitySettings.vSyncCount = option.UseVSync ? VSyncEveryFrame : VSyncOff;
            Application.targetFrameRate = option.UseVSync ? UnlimitedFrameRate : option.TargetFrameRate;
        }
    }

}
