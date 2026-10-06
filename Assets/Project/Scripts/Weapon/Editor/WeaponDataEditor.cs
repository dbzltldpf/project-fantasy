using System.Collections.Generic;
using ProjectFantasy.Magic;
using ProjectFantasy.Weapon;
using UnityEditor;
using UnityEngine;

namespace ProjectFantasy.WeaponEditor
{
    // 무기 데이터 인스펙터: 요약·설정 경고 + 무기 종류에 맞는 섹션만 표시 (데이터 형식은 그대로)
    [CustomEditor(typeof(WeaponData), true)]
    public sealed class WeaponDataEditor : Editor
    {
        private const string FoldoutKeyPrefix = "WeaponDataEditor.";
        private const string Separator = " · ";
        private const string MinField = "min";
        private const string MaxField = "max";

        private const string AttackRangeField = "attackPowerRange";
        private const string MagicRangeField = "magicPowerRange";
        private const string GradeTableField = "gradeTable";
        private const string ModelField = "modelPrefab";
        private const string ComboField = "comboData";
        private const string BladeBaseField = "bladeBase";
        private const string BladeTipField = "bladeTip";
        private const string AmmoField = "ammo";
        private const string SpellField = "spell";

        private static readonly string[] InfoFields = { "displayName", "icon", "description", "worldModelPrefab" };
        private static readonly string[] EquipFields = { "weaponType", "gripHand", "occupiesOffHand", "allowsShield", ModelField, "gripPosition", "gripRotation" };
        private static readonly string[] MeleeFields = { ComboField, "hitOffset", "hitRadius", BladeBaseField, BladeTipField, "bladeRadius" };
        private static readonly string[] RangedFields = { AmmoField, "canAim", "requiresReload", "muzzleOffset", "launchSpeed", "windupStateName", "windupDuration", "fireStateName", "releaseTime", "fireDuration" };
        private static readonly string[] AimFields = { "aimIdleStateName", "aimHoldTime" };
        private static readonly string[] ReloadFields = { "reloadStateName", "reloadDuration", "loadedAmmoPosition", "loadedAmmoRotation" };
        private static readonly string[] MagicFields = { SpellField, "muzzleOffset" };
        private static readonly string[] AnimationFields = { "idleStateName" };
        // 장비는 항상 1개씩이라 숨김
        private static readonly string[] HiddenFields = { "m_Script", "maxStack" };

        private static readonly HashSet<string> KnownFields = BuildKnownFields();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            WeaponData weapon = (WeaponData)target;
            bool isMagic = weapon is MagicWeaponData;

            EditorGUILayout.HelpBox(BuildSummary(weapon, isMagic), MessageType.None);
            DrawWarnings(weapon, isMagic);

            DrawSection("기본 정보", InfoFields);
            DrawSection("장착", EquipFields);
            DrawSection("능력치", GradeTableField, isMagic ? MagicRangeField : AttackRangeField);
            if (weapon.HasMeleeHit) DrawSection("근접 (콤보·판정)", MeleeFields);
            if (weapon is RangedWeaponData ranged) DrawRangedSection(ranged);
            if (isMagic) DrawSection("마법", MagicFields);
            DrawSection("애니메이션", AnimationFields);
            DrawUnknownFields();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRangedSection(RangedWeaponData ranged)
        {
            if (!BeginSection("원거리 (발사·조준·장전)")) return;

            DrawFields(RangedFields);
            if (ranged.CanAim) DrawFields(AimFields);
            if (ranged.RequiresReload) DrawFields(ReloadFields);
            EndSection();
        }

        private void DrawSection(string title, params string[] fields)
        {
            if (!BeginSection(title)) return;

            DrawFields(fields);
            EndSection();
        }

        private static bool BeginSection(string title)
        {
            string key = FoldoutKeyPrefix + title;
            bool isOpen = EditorGUILayout.BeginFoldoutHeaderGroup(SessionState.GetBool(key, true), title);
            SessionState.SetBool(key, isOpen);
            if (!isOpen)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
                return false;
            }

            EditorGUI.indentLevel++;
            return true;
        }

        private static void EndSection()
        {
            EditorGUI.indentLevel--;
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawFields(string[] fields)
        {
            foreach (string field in fields)
            {
                SerializedProperty property = serializedObject.FindProperty(field);
                if (property != null) EditorGUILayout.PropertyField(property, true);
            }
        }

        // 섹션에 등록되지 않은 새 필드는 누락되지 않도록 '기타'에 표시
        private void DrawUnknownFields()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool hasUnknown = false;

            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if (KnownFields.Contains(iterator.name)) continue;

                if (!hasUnknown)
                {
                    EditorGUILayout.LabelField("기타", EditorStyles.boldLabel);
                    hasUnknown = true;
                }
                EditorGUILayout.PropertyField(iterator, true);
            }
        }

        // 예: 한손검 · 오른손 · 보조: 방패 · 공격력 12~18 · 등급표 ✔
        private string BuildSummary(WeaponData weapon, bool isMagic)
        {
            string statLabel = isMagic ? "마법력" : "공격력";
            string range = FormatRange(isMagic ? MagicRangeField : AttackRangeField);
            bool hasGradeTable = serializedObject.FindProperty(GradeTableField).objectReferenceValue != null;

            return GetTypeLabel(weapon)
                + Separator + (weapon.IsHeldInLeftHand ? "왼손" : "오른손")
                + Separator + GetOffHandLabel(weapon)
                + Separator + $"{statLabel} {range}"
                + Separator + (hasGradeTable ? "등급표 ✔" : "등급표 없음");
        }

        private void DrawWarnings(WeaponData weapon, bool isMagic)
        {
            bool isUnarmed = weapon.WeaponType == WeaponType.Unarmed;
            bool isBladeWeapon = weapon.WeaponType == WeaponType.OneHanded || weapon.WeaponType == WeaponType.TwoHanded;

            if (!isUnarmed && IsMissing(ModelField)) Warn("모델 프리팹이 없어 손에 아무것도 보이지 않습니다.");
            if (weapon.HasMeleeHit && IsMissing(ComboField)) Warn("콤보가 없어 근접 공격을 할 수 없습니다.");
            if (weapon is RangedWeaponData && IsMissing(AmmoField)) Warn("화살(Ammo)이 없어 발사할 수 없습니다.");
            if (isMagic && IsMissing(SpellField)) Warn("마법(Spell)이 없어 시전할 수 없습니다.");
            if (IsInvertedRange(isMagic ? MagicRangeField : AttackRangeField)) Warn("능력치 범위의 max가 min보다 작습니다 (min으로 고정됨).");
            if (isBladeWeapon && IsBladeUnset()) Info("칼날 미설정: 몸 기준 구체로 판정합니다 (Blade Base/Tip 입력 권장).");
            if (!isUnarmed && IsMissing(GradeTableField)) Info("등급표가 없어 등급 없이 범위 값만 굴립니다.");
        }

        private bool IsMissing(string field) => serializedObject.FindProperty(field)?.objectReferenceValue == null;

        private bool IsBladeUnset()
        {
            return serializedObject.FindProperty(BladeBaseField).vector3Value == serializedObject.FindProperty(BladeTipField).vector3Value;
        }

        private bool IsInvertedRange(string field)
        {
            SerializedProperty range = serializedObject.FindProperty(field);
            return range != null && range.FindPropertyRelative(MaxField).intValue < range.FindPropertyRelative(MinField).intValue;
        }

        private string FormatRange(string field)
        {
            SerializedProperty range = serializedObject.FindProperty(field);
            if (range == null) return "-";

            int min = range.FindPropertyRelative(MinField).intValue;
            int max = Mathf.Max(min, range.FindPropertyRelative(MaxField).intValue);
            return min == max ? min.ToString() : $"{min}~{max}";
        }

        private static string GetTypeLabel(WeaponData weapon)
        {
            switch (weapon.WeaponType)
            {
                case WeaponType.Unarmed: return "맨손";
                case WeaponType.OneHanded: return "한손검";
                case WeaponType.TwoHanded: return "양손검";
                case WeaponType.Wand: return "Wand (한손 마법)";
                case WeaponType.Staff: return "Staff (범위 마법)";
                case WeaponType.Bow: return "활";
                case WeaponType.Crossbow: return weapon.OccupiesOffHand ? "양손 석궁" : "한손 석궁";
                default: return weapon.WeaponType.ToString();
            }
        }

        private static string GetOffHandLabel(WeaponData weapon)
        {
            if (weapon.OccupiesOffHand) return "보조 장비 불가";
            if (weapon.AllowsShield && weapon.IsMagic) return "보조: 방패·마법서";
            if (weapon.AllowsShield) return "보조: 방패";
            return weapon.IsMagic ? "보조: 마법서" : "보조 장비 불가";
        }

        private static void Warn(string message) => EditorGUILayout.HelpBox(message, MessageType.Warning);
        private static void Info(string message) => EditorGUILayout.HelpBox(message, MessageType.Info);

        private static HashSet<string> BuildKnownFields()
        {
            HashSet<string> fields = new HashSet<string> { GradeTableField, AttackRangeField, MagicRangeField };
            foreach (string[] group in new[] { InfoFields, EquipFields, MeleeFields, RangedFields, AimFields, ReloadFields, MagicFields, AnimationFields, HiddenFields })
            {
                fields.UnionWith(group);
            }
            return fields;
        }
    }
}
