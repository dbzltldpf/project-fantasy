using ProjectFantasy.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectFantasy.UIEditor
{
    // 플레이어 ↔ UI 연결(Presenter)을 캔버스 아래 전용 오브젝트 한 곳에 모음 (UI 루트를 다시 만들어도 연결 유지)
    internal static class UIPresenterSetup
    {
        public const string RootName = "UI Presenters";
        private const string MenuPath = "Tools/ProjectFantasy/Gather UI Presenters";
        private const string UndoName = "Gather UI Presenters";
        private const int FirstSiblingIndex = 0;
        private const int TransformOnly = 1;

        [MenuItem(MenuPath)]
        private static void GatherFromMenu()
        {
            Canvas canvas = UIBuilderUtility.FindOrCreateCanvas(UndoName);
            GameObject root = Gather(canvas, UndoName);
            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = root;
            Debug.Log($"[{nameof(UIPresenterSetup)}] Presenter를 '{RootName}'에 모았습니다.");
        }

        // 씬에 흩어진 Presenter를 값 그대로 옮김 (UI 루트 삭제 전에 호출)
        public static GameObject Gather(Canvas canvas, string undoName)
        {
            GameObject root = GetOrCreateRoot(canvas, undoName);
            Move<PlayerAimPresenter>(root);
            Move<PlayerMenuPresenter>(root);
            Move<PlayerHudPresenter>(root);
            return root;
        }

        public static GameObject GetOrCreateRoot(Canvas canvas, string undoName)
        {
            Transform existing = canvas.transform.Find(RootName);
            if (existing != null) return existing.gameObject;

            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, undoName);
            root.transform.SetParent(canvas.transform, false);
            root.transform.SetSiblingIndex(FirstSiblingIndex);
            return root;
        }

        public static T GetOrAdd<T>(GameObject root) where T : Component
        {
            return root.TryGetComponent(out T component) ? component : Undo.AddComponent<T>(root);
        }

        private static void Move<T>(GameObject root) where T : Component
        {
            T source = Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
            if (source == null || source.gameObject == root) return;

            T target = GetOrAdd<T>(root);
            Undo.RecordObject(target, UndoName);
            EditorUtility.CopySerialized(source, target);

            GameObject oldObject = source.gameObject;
            Undo.DestroyObjectImmediate(source);

            // Presenter만 있던 빈 오브젝트는 정리
            bool isEmpty = oldObject.GetComponents<Component>().Length == TransformOnly && oldObject.transform.childCount == 0;
            if (isEmpty) Undo.DestroyObjectImmediate(oldObject);
        }
    }
}
