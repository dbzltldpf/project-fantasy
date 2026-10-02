using System;
using ProjectFantasy.Combat;
using UnityEngine;

namespace ProjectFantasy.CombatEditor
{
    // 이벤트 종류별 트랙 색 (등록되지 않은 새 이벤트는 기본 색)
    internal static class ActionEventStyle
    {
        private static readonly Color HitColor = new Color(0.85f, 0.3f, 0.3f);
        private static readonly Color ComboColor = new Color(0.3f, 0.55f, 0.9f);
        private static readonly Color MoveColor = new Color(0.35f, 0.75f, 0.4f);
        private static readonly Color DefaultColor = new Color(0.6f, 0.6f, 0.6f);

        public static Color GetColor(Type eventType)
        {
            if (eventType == typeof(HitWindowEvent)) return HitColor;
            if (eventType == typeof(ComboWindowEvent)) return ComboColor;
            if (eventType == typeof(MoveEvent)) return MoveColor;
            return DefaultColor;
        }
    }
}
