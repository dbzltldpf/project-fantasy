using ProjectFantasy.Combat;
using ProjectFantasy.Player;
using ProjectFantasy.Weapon;
using UnityEditor;
using UnityEngine;

namespace ProjectFantasy.CombatEditor
{
    // 액션 타임라인 편집 창: 대상 선택, 재생 제어, 트랙 편집, Scene 뷰 미리보기 연결
    public sealed class ActionTimelineWindow : EditorWindow
    {
        private const string WindowTitle = "Action Timeline";
        private const string MenuPath = "Window/ProjectFantasy/Action Timeline";
        private const string WeaponFilter = "t:" + nameof(WeaponData);
        private const string PlayIconName = "PlayButton";
        private const string PauseIconName = "PauseButton";
        private const string PrevIconName = "Animation.PrevKey";
        private const string NextIconName = "Animation.NextKey";
        private const float ButtonWidth = 30f;
        private const int StepFrames = 1;
        private const int MinTotalFrames = 1;

        [SerializeField] private ActionData action;
        [SerializeField] private GameObject previewTarget;
        [SerializeField] private WeaponData previewWeapon;
        [SerializeField] private bool isPreviewEnabled = true;
        [SerializeField] private float currentFrame;

        private SerializedObject actionObject;
        private ActionTimelineTrackView trackView;
        private ActionPreview preview;
        private Vector2 detailScroll;
        private bool isPlaying;
        private double lastUpdateTime;

        [MenuItem(MenuPath)]
        private static void OpenFromMenu() => GetWindow<ActionTimelineWindow>(WindowTitle);

        public static void Open(ActionData target)
        {
            ActionTimelineWindow window = GetWindow<ActionTimelineWindow>(WindowTitle);
            window.SetAction(target);
        }

        private void OnEnable()
        {
            trackView = new ActionTimelineTrackView();
            trackView.Changed += Repaint;
            preview = new ActionPreview();

            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Undo.undoRedoPerformed += OnUndoRedo;

            if (action != null) actionObject = new SerializedObject(action);
        }

        private void OnDisable()
        {
            trackView.Changed -= Repaint;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Undo.undoRedoPerformed -= OnUndoRedo;
            preview.Dispose();
        }

        // 프로젝트에서 ActionData를 선택하면 따라감
        private void OnSelectionChange()
        {
            if (Selection.activeObject is ActionData selected && selected != action) SetAction(selected);
        }

        private void OnGUI()
        {
            DrawTargetFields();

            if (action == null)
            {
                EditorGUILayout.HelpBox("ActionData를 선택하거나 인스펙터의 '타임라인 열기'를 누르세요.", MessageType.Info);
                preview.Stop();
                return;
            }

            actionObject.Update();
            int clipFrames = ActionEditorUtility.GetClipFrameCount(action);
            int totalFrames = Mathf.Max(MinTotalFrames, action.LengthFrames, clipFrames);

            DrawPlaybackBar(clipFrames, totalFrames);

            int eventCount = actionObject.FindProperty(ActionEditorUtility.EventsPropertyName).arraySize;
            Rect trackRect = GUILayoutUtility.GetRect(0f, ActionTimelineTrackView.GetHeight(eventCount), GUILayout.ExpandWidth(true));
            currentFrame = trackView.Draw(trackRect, action, actionObject, currentFrame, totalFrames);

            DrawSelectedEvent();
            actionObject.ApplyModifiedProperties();

            if (Event.current.type == EventType.Repaint) RefreshPreview();
        }

        private void SetAction(ActionData target)
        {
            action = target;
            actionObject = target != null ? new SerializedObject(target) : null;
            currentFrame = 0f;
            isPlaying = false;
            trackView.SelectedIndex = ActionTimelineTrackView.NoSelection;

            if (target != null && (previewWeapon == null || previewWeapon.ComboData == null || !ContainsAction(previewWeapon.ComboData, target)))
            {
                previewWeapon = FindWeaponUsing(target);
            }
            Repaint();
        }

        private void DrawTargetFields()
        {
            ActionData selected = (ActionData)EditorGUILayout.ObjectField("Action", action, typeof(ActionData), false);
            if (selected != action) SetAction(selected);

            if (previewTarget == null) previewTarget = FindDefaultTarget();
            previewTarget = (GameObject)EditorGUILayout.ObjectField("미리보기 캐릭터", previewTarget, typeof(GameObject), true);
            previewWeapon = (WeaponData)EditorGUILayout.ObjectField("미리보기 무기", previewWeapon, typeof(WeaponData), false);
            isPreviewEnabled = EditorGUILayout.Toggle("Scene 미리보기", isPreviewEnabled);

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUILayout.HelpBox("플레이 중에는 Scene 미리보기를 사용할 수 없습니다 (트랙 편집은 가능).", MessageType.None);
            }
        }

        private void DrawPlaybackBar(int clipFrames, int totalFrames)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(EditorGUIUtility.IconContent(PrevIconName), GUILayout.Width(ButtonWidth))) StepFrame(-StepFrames, totalFrames);
            if (GUILayout.Button(EditorGUIUtility.IconContent(isPlaying ? PauseIconName : PlayIconName), GUILayout.Width(ButtonWidth))) TogglePlay();
            if (GUILayout.Button(EditorGUIUtility.IconContent(NextIconName), GUILayout.Width(ButtonWidth))) StepFrame(StepFrames, totalFrames);

            float seconds = currentFrame / action.FrameRate;
            GUILayout.Label($"Frame {currentFrame:0} / {action.LengthFrames}   ({seconds:0.00}s)   Clip {clipFrames}f @ {action.FrameRate:0}fps   Speed x{action.PlaybackSpeed:0.##}");
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSelectedEvent()
        {
            SerializedProperty events = actionObject.FindProperty(ActionEditorUtility.EventsPropertyName);
            int index = trackView.SelectedIndex;

            EditorGUILayout.Space();
            detailScroll = EditorGUILayout.BeginScrollView(detailScroll);

            if (index < 0 || index >= events.arraySize)
            {
                EditorGUILayout.HelpBox("트랙 클릭: 선택 · 막대 드래그: 이동 · 양 끝: 길이 · 흰 마커: 전이 프레임 · 노란 선: 액션 길이 · 우클릭: 추가/삭제 · 눈금자: 프레임 이동", MessageType.None);
            }
            else
            {
                SerializedProperty element = events.GetArrayElementAtIndex(index);
                EditorGUILayout.LabelField($"{index}. {ActionEditorUtility.GetEventName(element)}", EditorStyles.boldLabel);
                ActionEditorUtility.DrawChildren(element);

                string warning = ActionEditorUtility.GetWarning(action, index);
                if (warning != null) EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();
        }

        private void RefreshPreview()
        {
            if (isPreviewEnabled && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                preview.Refresh(action, previewTarget, previewWeapon, currentFrame);
            }
            else
            {
                preview.Stop();
            }
        }

        private void TogglePlay()
        {
            isPlaying = !isPlaying;
            lastUpdateTime = EditorApplication.timeSinceStartup;
        }

        private void StepFrame(int direction, int totalFrames)
        {
            isPlaying = false;
            currentFrame = Mathf.Clamp(Mathf.Round(currentFrame) + direction, 0f, totalFrames);
        }

        // 재생 속도 배율을 반영해 액션 길이 안에서 반복 재생
        private void OnEditorUpdate()
        {
            if (!isPlaying || action == null) return;

            double now = EditorApplication.timeSinceStartup;
            float deltaTime = (float)(now - lastUpdateTime);
            lastUpdateTime = now;

            currentFrame += deltaTime * action.FrameRate * action.PlaybackSpeed;
            if (currentFrame >= action.LengthFrames) currentFrame = 0f;
            Repaint();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode) return;

            isPlaying = false;
            preview.Stop();
        }

        private void OnUndoRedo()
        {
            actionObject?.Update();
            Repaint();
        }

        private static GameObject FindDefaultTarget()
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            return player != null ? player.gameObject : null;
        }

        // 이 액션을 콤보로 쓰는 무기 (칼날 미리보기용)
        private static WeaponData FindWeaponUsing(ActionData target)
        {
            foreach (string guid in AssetDatabase.FindAssets(WeaponFilter))
            {
                WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(AssetDatabase.GUIDToAssetPath(guid));
                if (weapon != null && weapon.ComboData != null && ContainsAction(weapon.ComboData, target)) return weapon;
            }
            return null;
        }

        private static bool ContainsAction(AttackComboData combo, ActionData target)
        {
            for (int i = 0; i < combo.ActionCount; i++)
            {
                if (combo.GetAction(i) == target) return true;
            }
            return false;
        }
    }
}
