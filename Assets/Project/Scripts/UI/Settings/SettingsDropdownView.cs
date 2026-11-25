using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace ProjectFantasy.UI
{
    // 설정 드롭다운 표시, 선택 알림 (적용·저장은 모름)
    public sealed class SettingsDropdownView : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;
        public event Action<int> Selected;
        private void Awake()
        {
            dropdown.onValueChanged.AddListener(index => Selected?.Invoke(index));
        }
        public void SetOptions(List<string> labels, int selectedIndex)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(labels);
            dropdown.SetValueWithoutNotify(selectedIndex);
        }
    }
}

