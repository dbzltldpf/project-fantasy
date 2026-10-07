using System.Collections.Generic;
using System.Text;
using ProjectFantasy.Items;
using TMPro;
using UnityEngine;

namespace ProjectFantasy.UI
{
    // 짧은 안내 문구 (여러 줄 동시 표시, 줄마다 일정 시간 후 사라짐, 최대 줄 수 초과 시 오래된 줄부터 제거)
    [DisallowMultipleComponent]
    public sealed class NoticeView : MonoBehaviour
    {
        private readonly struct NoticeLine
        {
            public readonly string Text;
            public readonly float HideTime;

            public NoticeLine(string text, float hideTime)
            {
                Text = text;
                HideTime = hideTime;
            }
        }

        [Tooltip("비우면 자신")]
        [SerializeField] private GameObject root;
        [Tooltip("문구 텍스트")]
        [SerializeField] private TMP_Text label;
        [Tooltip("한 줄이 보이는 시간 (초)")]
        [SerializeField, Min(0f)] private float displayDuration = 1.5f;
        [Tooltip("동시에 보이는 최대 줄 수")]
        [SerializeField, Min(1)] private int maxLines = 4;

        [Header("Messages")]
        [SerializeField] private string inventoryFullMessage = "가방이 가득 찼습니다";
        [SerializeField] private string cannotUseNowMessage = "지금은 사용할 수 없습니다";
        [SerializeField] private string offHandIncompatibleMessage = "현재 무기와 함께 장착할 수 없습니다";
        [SerializeField] private string offHandDisabledMessage = "보조 장비가 비활성화되었습니다";
        [SerializeField] private string masteryTooLowMessage = "숙련도가 부족합니다";

        private readonly List<NoticeLine> lines = new List<NoticeLine>();
        private readonly StringBuilder builder = new StringBuilder();

        private void Awake()
        {
            if (root == null) root = gameObject;
            root.SetActive(false);
        }

        private void Update()
        {
            int removed = lines.RemoveAll(line => Time.time >= line.HideTime);
            if (removed > 0) Rebuild();
        }

        public void Show(ItemNotice notice) => ShowMessage(GetMessage(notice));

        public void ShowMessage(string message)
        {
            if (lines.Count >= maxLines) lines.RemoveAt(0);

            lines.Add(new NoticeLine(message, Time.time + displayDuration));
            Rebuild();
        }

        private void Rebuild()
        {
            builder.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                if (i > 0) builder.AppendLine();
                builder.Append(lines[i].Text);
            }

            label.SetText(builder);
            root.SetActive(lines.Count > 0);
        }

        private string GetMessage(ItemNotice notice)
        {
            switch (notice)
            {
                case ItemNotice.InventoryFull: return inventoryFullMessage;
                case ItemNotice.OffHandIncompatible: return offHandIncompatibleMessage;
                case ItemNotice.OffHandDisabled: return offHandDisabledMessage;
                case ItemNotice.MasteryTooLow: return masteryTooLowMessage;
                default: return cannotUseNowMessage;
            }
        }
    }
}
