using System;
using System.Collections.Generic;
using ProjectFantasy.Combat;
using ProjectFantasy.Items;
using ProjectFantasy.Weapon;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFantasy.CombatEditor
{
    // 씬 캐릭터에 클립 프레임 샘플링(AnimationMode), 임시 무기 부착, 칼날 궤적 캐싱·Scene 뷰 표시
    internal sealed class ActionPreview : IDisposable
    {
        private const float NotSampled = -1f;
        private const int InvalidKey = 0;
        private const int QuadVertexCount = 4;
        private const int SegmentVertexCount = 2;
        private const float PathWidth = 2f;
        private const float HitOutlineWidth = 4f;
        private const float BladeWidth = 6f;

        private static readonly Color PathColor = new Color(1f, 1f, 1f, 0.35f);
        private static readonly Color HitFillColor = new Color(1f, 0.25f, 0.2f, 0.3f);
        private static readonly Color HitOutlineColor = new Color(1f, 0.3f, 0.2f, 0.9f);
        private static readonly Color BladeColor = new Color(1f, 0.85f, 0.2f);
        private static readonly Color ActiveHitColor = new Color(1f, 0.2f, 0.2f);
        private static readonly Color IdleHitColor = new Color(1f, 1f, 1f, 0.4f);

        private readonly AnimationModeDriver driver;
        private readonly List<Vector3> baseLocalPath = new List<Vector3>();
        private readonly List<Vector3> tipLocalPath = new List<Vector3>();
        private readonly Vector3[] quad = new Vector3[QuadVertexCount];
        private readonly Vector3[] segment = new Vector3[SegmentVertexCount];
        private Vector3[] pathBuffer = Array.Empty<Vector3>();

        private ActionData action;
        private WeaponData weapon;
        private Animator animator;
        private Transform root;
        private GameObject weaponInstance;
        private int cacheKey = InvalidKey;
        private float sampledFrame = NotSampled;

        private bool HasBlade => weaponInstance != null && weapon.HasBlade;

        public ActionPreview()
        {
            driver = ScriptableObject.CreateInstance<AnimationModeDriver>();
            driver.hideFlags = HideFlags.HideAndDontSave;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public void Dispose()
        {
            Stop();
            SceneView.duringSceneGui -= OnSceneGUI;
            Object.DestroyImmediate(driver);
        }

        // 대상·무기·데이터가 바뀌었을 때만 재구성, 프레임이 바뀌었을 때만 샘플링
        public void Refresh(ActionData targetAction, GameObject target, WeaponData targetWeapon, float frame)
        {
            Animator targetAnimator = target != null ? target.GetComponentInChildren<Animator>() : null;
            if (targetAction == null || targetAction.Clip == null || targetAnimator == null)
            {
                Stop();
                return;
            }

            if (targetAnimator != animator || targetWeapon != weapon)
            {
                DestroyWeaponInstance();
                animator = targetAnimator;
                root = target.transform;
                weapon = targetWeapon;
                CreateWeaponInstance();
                cacheKey = InvalidKey;
            }

            action = targetAction;

            if (!AnimationMode.InAnimationMode(driver))
            {
                AnimationMode.StartAnimationMode(driver);
                cacheKey = InvalidKey;
            }

            int key = ComputeCacheKey();
            if (key != cacheKey)
            {
                RebuildPath();
                cacheKey = key;
                sampledFrame = NotSampled;
            }

            if (Mathf.Approximately(frame, sampledFrame)) return;

            Sample(frame);
            sampledFrame = frame;
            SceneView.RepaintAll();
        }

        // 원래 자세로 복구하고 임시 오브젝트 정리
        public void Stop()
        {
            if (AnimationMode.InAnimationMode(driver)) AnimationMode.StopAnimationMode(driver);

            DestroyWeaponInstance();
            action = null;
            weapon = null;
            animator = null;
            root = null;
            baseLocalPath.Clear();
            tipLocalPath.Clear();
            cacheKey = InvalidKey;
            sampledFrame = NotSampled;
            SceneView.RepaintAll();
        }

        private int ComputeCacheKey()
        {
            Vector3 bladeBase = weapon != null ? weapon.BladeBase : Vector3.zero;
            Vector3 bladeTip = weapon != null ? weapon.BladeTip : Vector3.zero;
            return HashCode.Combine(action.Clip, action.Clip.length, bladeBase, bladeTip, weaponInstance);
        }

        private void Sample(float frame)
        {
            AnimationClip clip = action.Clip;
            float time = Mathf.Min(frame / clip.frameRate, clip.length);

            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(animator.gameObject, clip, time);
            AnimationMode.EndSampling();
        }

        // 클립 전 프레임의 칼날 시작·끝을 캐릭터 로컬로 기록 (캐릭터를 옮겨도 유효)
        private void RebuildPath()
        {
            baseLocalPath.Clear();
            tipLocalPath.Clear();
            if (!HasBlade) return;

            Transform blade = weaponInstance.transform;
            int frameCount = ActionEditorUtility.GetClipFrameCount(action);

            for (int frame = 0; frame <= frameCount; frame++)
            {
                Sample(frame);
                baseLocalPath.Add(root.InverseTransformPoint(blade.TransformPoint(weapon.BladeBase)));
                tipLocalPath.Add(root.InverseTransformPoint(blade.TransformPoint(weapon.BladeTip)));
            }

            if (pathBuffer.Length < tipLocalPath.Count) pathBuffer = new Vector3[tipLocalPath.Count];
        }

        private void CreateWeaponInstance()
        {
            if (weapon == null || weapon.ModelPrefab == null) return;

            EquipmentVisual visual = root.GetComponentInChildren<EquipmentVisual>();
            Transform slot = visual != null ? visual.GetHandSlot(weapon.GripHand) : null;
            if (slot == null) return;

            weaponInstance = Object.Instantiate(weapon.ModelPrefab, slot);
            weapon.ApplyModelVisual(weaponInstance, ItemData.MinTier);
            weaponInstance.transform.SetLocalPositionAndRotation(weapon.GripPosition, weapon.GripRotation);

            // 씬에 저장되지 않는 미리보기 전용
            foreach (Transform child in weaponInstance.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void DestroyWeaponInstance()
        {
            if (weaponInstance == null) return;

            Object.DestroyImmediate(weaponInstance);
            weaponInstance = null;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (action == null || root == null || Event.current.type != EventType.Repaint) return;

            bool isHitActive = IsInHitWindow(sampledFrame);

            if (HasBlade && tipLocalPath.Count > 0)
            {
                DrawBladePath();
                DrawCurrentBlade(isHitActive);
            }
            else if (weapon != null)
            {
                DrawHitSphere(isHitActive);
            }
        }

        private void DrawBladePath()
        {
            int count = tipLocalPath.Count;
            for (int i = 0; i < count; i++)
            {
                pathBuffer[i] = root.TransformPoint(tipLocalPath[i]);
            }

            Handles.color = PathColor;
            Handles.DrawAAPolyLine(PathWidth, count, pathBuffer);

            for (int i = 0; i < action.EventCount; i++)
            {
                if (action.GetEvent(i) is HitWindowEvent hitWindow) DrawHitSurface(hitWindow.StartFrame, hitWindow.EndFrame);
            }
        }

        // 타격 구간 동안 칼날이 쓸고 지나가는 면
        private void DrawHitSurface(int startFrame, int endFrame)
        {
            int lastFrame = Mathf.Min(endFrame, tipLocalPath.Count - 1);

            for (int frame = Mathf.Max(0, startFrame); frame < lastFrame; frame++)
            {
                quad[0] = root.TransformPoint(baseLocalPath[frame]);
                quad[1] = root.TransformPoint(tipLocalPath[frame]);
                quad[2] = root.TransformPoint(tipLocalPath[frame + 1]);
                quad[3] = root.TransformPoint(baseLocalPath[frame + 1]);

                Handles.color = HitFillColor;
                Handles.DrawAAConvexPolygon(quad);

                segment[0] = quad[1];
                segment[1] = quad[2];
                Handles.color = HitOutlineColor;
                Handles.DrawAAPolyLine(HitOutlineWidth, segment);
            }
        }

        private void DrawCurrentBlade(bool isHitActive)
        {
            Transform blade = weaponInstance.transform;
            segment[0] = blade.TransformPoint(weapon.BladeBase);
            segment[1] = blade.TransformPoint(weapon.BladeTip);

            Handles.color = isHitActive ? ActiveHitColor : BladeColor;
            Handles.DrawAAPolyLine(BladeWidth, segment);
        }

        // 칼날 정보가 없는 무기는 몸 기준 판정 구체
        private void DrawHitSphere(bool isHitActive)
        {
            Vector3 center = root.TransformPoint(weapon.HitOffset);
            float radius = weapon.HitRadius;

            Handles.color = isHitActive ? ActiveHitColor : IdleHitColor;
            Handles.DrawWireDisc(center, Vector3.up, radius);
            Handles.DrawWireDisc(center, root.forward, radius);
            Handles.DrawWireDisc(center, root.right, radius);
        }

        private bool IsInHitWindow(float frame)
        {
            for (int i = 0; i < action.EventCount; i++)
            {
                if (action.GetEvent(i) is HitWindowEvent hitWindow && hitWindow.Contains(frame)) return true;
            }
            return false;
        }
    }
}
