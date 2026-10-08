using System.Collections.Generic;
using System.Text;
using ProjectFantasy.Combat;
using ProjectFantasy.Enemy;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ProjectFantasy.EnemyEditor
{
    // 선택한 EnemyData에 필요한 상태(이동·피격·사망·등장·막기 + 전투 방식별 공격 상태)를 모아 Animator Controller 생성
    // 상태 이름 = 클립 이름, 전이선 없음 (EnemyAnimator가 이름으로 CrossFade), 다시 실행하면 같은 에셋을 갱신
    public static class EnemyAnimatorBuilder
    {
        private const string MenuPath = "Tools/ProjectFantasy/Create Enemy Animator";
        private const string Title = "Create Enemy Animator";
        private const string ClipFolder = "Assets/Project/Animation/Clip";
        private const string ControllerFolder = "Assets/Project/Animation/Controller";
        private const string ControllerPrefix = "Enemy_";
        private const string ControllerExtension = ".controller";
        private const float DefaultParameterSpeed = 1f;
        private const int BaseLayerIndex = 0;

        [MenuItem(MenuPath)]
        private static void Create()
        {
            EnemyData[] selected = Selection.GetFiltered<EnemyData>(SelectionMode.Assets);
            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog(Title, "Project 창에서 EnemyData 에셋을 선택한 뒤 실행하세요 (여러 개 선택 가능).", "확인");
                return;
            }

            Dictionary<string, AnimationClip> clipsByName = LoadClips();
            foreach (EnemyData data in selected)
            {
                Build(data, clipsByName);
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem(MenuPath, true)]
        private static bool CanCreate() => Selection.GetFiltered<EnemyData>(SelectionMode.Assets).Length > 0;

        private static void Build(EnemyData data, Dictionary<string, AnimationClip> clipsByName)
        {
            if (data.AnimationData == null)
            {
                Debug.LogError($"[{nameof(EnemyAnimatorBuilder)}] '{data.name}'에 Animation Data가 없습니다.", data);
                return;
            }

            // 상태 이름 → 클립 (ActionData는 지정 클립 우선), 삽입 순서 유지
            List<string> stateNames = new List<string>();
            Dictionary<string, AnimationClip> stateClips = new Dictionary<string, AnimationClip>();
            HashSet<string> actionStates = new HashSet<string>();

            string idleState = data.Weapon != null && !string.IsNullOrEmpty(data.Weapon.IdleStateName) ? data.Weapon.IdleStateName : data.AnimationData.DefaultIdleState;
            AddState(idleState, null, clipsByName, stateNames, stateClips);
            foreach (string state in data.AnimationData.GetBaseStates())
            {
                AddState(state, null, clipsByName, stateNames, stateClips);
            }

            // 전투 방식별 상태 (근접 콤보·반격 / 조준·발사·장전 / 마법 시전)
            List<string> styleStates = new List<string>();
            List<ActionData> styleActions = new List<ActionData>();
            data.CombatStyle.CollectAnimationStates(data, styleStates, styleActions);
            HashSet<string> holdStates = new HashSet<string>();
            data.CombatStyle.CollectHoldStates(data, holdStates);
            foreach (string state in styleStates)
            {
                AddState(state, null, clipsByName, stateNames, stateClips);
            }
            foreach (ActionData action in styleActions)
            {
                AddAction(action, clipsByName, stateNames, stateClips, actionStates);
            }

            AnimatorController controller = LoadOrCreateController(data);
            AnimatorStateMachine stateMachine = controller.layers[BaseLayerIndex].stateMachine;
            ClearStates(stateMachine);
            EnsureFloatParameter(controller, data.AnimationData.ActionSpeedParameter);
            EnsureFloatParameter(controller, data.AnimationData.PoseSpeedParameter);

            StringBuilder missing = new StringBuilder();
            foreach (string stateName in stateNames)
            {
                AnimatorState state = stateMachine.AddState(stateName);
                state.motion = stateClips[stateName];
                if (state.motion == null) missing.Append(' ').Append(stateName);

                if (actionStates.Contains(stateName))
                {
                    state.speedParameter = data.AnimationData.ActionSpeedParameter;
                    state.speedParameterActive = true;
                }
                else if (holdStates.Contains(stateName))
                {
                    state.speedParameter = data.AnimationData.PoseSpeedParameter;
                    state.speedParameterActive = true;
                }
                if (stateName == idleState) stateMachine.defaultState = state;
            }

            EditorUtility.SetDirty(controller);
            EditorGUIUtility.PingObject(controller);
            Debug.Log($"[{nameof(EnemyAnimatorBuilder)}] '{controller.name}' 상태 {stateNames.Count}개 생성 → Animator Controller에 연결하세요.", controller);
            if (missing.Length > 0)
            {
                Debug.LogWarning($"[{nameof(EnemyAnimatorBuilder)}] '{ClipFolder}'에서 클립을 찾지 못한 상태:{missing}", controller);
            }
        }

        private static void AddAction(ActionData action, Dictionary<string, AnimationClip> clipsByName,
            List<string> stateNames, Dictionary<string, AnimationClip> stateClips, HashSet<string> actionStates)
        {
            if (action == null || string.IsNullOrEmpty(action.StateName)) return;

            AddState(action.StateName, action.Clip, clipsByName, stateNames, stateClips);
            actionStates.Add(action.StateName);
        }

        private static void AddState(string stateName, AnimationClip preferredClip, Dictionary<string, AnimationClip> clipsByName,
            List<string> stateNames, Dictionary<string, AnimationClip> stateClips)
        {
            if (string.IsNullOrEmpty(stateName) || stateClips.ContainsKey(stateName)) return;

            if (preferredClip == null) clipsByName.TryGetValue(stateName, out preferredClip);
            stateNames.Add(stateName);
            stateClips.Add(stateName, preferredClip);
        }

        // Bake Into Pose를 적용해 추출한 .anim 클립 (플레이어와 동일 소스)
        private static Dictionary<string, AnimationClip> LoadClips()
        {
            Dictionary<string, AnimationClip> clips = new Dictionary<string, AnimationClip>();
            foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { ClipFolder }))
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid));
                if (clip != null && !clips.ContainsKey(clip.name)) clips.Add(clip.name, clip);
            }
            return clips;
        }

        // 기존 에셋을 지우지 않고 갱신 (프리팹의 Animator 연결 유지)
        private static AnimatorController LoadOrCreateController(EnemyData data)
        {
            string path = $"{ControllerFolder}/{ControllerPrefix}{data.name}{ControllerExtension}";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            return controller != null ? controller : AnimatorController.CreateAnimatorControllerAtPath(path);
        }

        private static void ClearStates(AnimatorStateMachine stateMachine)
        {
            foreach (ChildAnimatorState child in stateMachine.states)
            {
                stateMachine.RemoveState(child.state);
            }
        }

        private static void EnsureFloatParameter(AnimatorController controller, string parameterName)
        {
            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                if (parameter.name == parameterName) return;
            }

            controller.AddParameter(new AnimatorControllerParameter
            {
                name = parameterName,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = DefaultParameterSpeed
            });
        }
    }
}
