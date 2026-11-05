using System.Collections.Generic;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 부위 파괴 시 아웃라인 번쩍임 후 약하게 유지, 전투 초기화 시 제거
    [RequireComponent(typeof(BreakablePartManager), typeof(EnemyController))]
    [DisallowMultipleComponent]
    public sealed class BrokenPartOutline : MonoBehaviour
    {
        [Tooltip("부서진 부위 렌더러에 덧씌울 아웃라인 머티리얼")]
        [SerializeField] private Material outlineMaterial;
        [Tooltip("파괴 순간 번쩍이는 시간 (초)")]
        [SerializeField, Min(0.01f)] private float flashDuration = 3f;
        [Tooltip("번쩍일 때 아웃라인 색")]
        [SerializeField]
        [ColorUsage(false, true)]
        private Color flashColor = new Color(1f, 0.1f, 0.05f);
        [Tooltip("번쩍임 후 유지되는 약한 아웃라인 색")]
        [SerializeField]
        [ColorUsage(false, true)]
        private Color idleColor = new Color(0.5f, 0.05f, 0.02f);
        private EnemyController enemyController;
        private BreakablePartManager breakablePartManager;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock propertyBlock;
        private readonly Dictionary<Renderer, int> outlineSlotByRenderer = new Dictionary<Renderer, int>();
        private readonly List<ActiveFlash> activeFlashes = new List<ActiveFlash>();
        private readonly struct ActiveFlash
        {
            public readonly BreakablePart BreakablePart;
            public readonly float EndTime;

            public ActiveFlash(BreakablePart breakablePart, float endTime)
            {
                BreakablePart = breakablePart;
                EndTime = endTime;
            }
        }

        private void Awake()
        {
            enemyController = GetComponent<EnemyController>();
            breakablePartManager = GetComponent<BreakablePartManager>();
            propertyBlock = new MaterialPropertyBlock();

            if (outlineMaterial == null)
            {
                Debug.LogWarning($"[{name}] outlineMaterial 비어있음", this);
                return;
            }

            foreach (var breakablePart in breakablePartManager.BreakableParts)
            {
                foreach (var renderer in breakablePart.Renderers)
                {
                    if (renderer == null) continue;

                    Material[] baseMaterial = renderer.sharedMaterials;
                    Material[] newMaterial = new Material[baseMaterial.Length + 1];
                    System.Array.Copy(baseMaterial, newMaterial, baseMaterial.Length);
                    newMaterial[newMaterial.Length - 1] = outlineMaterial;
                    renderer.sharedMaterials = newMaterial;
                    outlineSlotByRenderer[renderer] = baseMaterial.Length;
                }
                SetOutLineColor(breakablePart, Color.black);

            }
        }
        private void OnEnable()
        {
            enemyController.CombatReset += HandleCombatReset;
            breakablePartManager.PartBroken += HandlePartBroken;
        }
        private void OnDisable()
        {
            enemyController.CombatReset -= HandleCombatReset;
            breakablePartManager.PartBroken -= HandlePartBroken;
        }
        private void HandlePartBroken(BreakablePart breakablePart)
        {
            activeFlashes.Add(new ActiveFlash(breakablePart, Time.time + flashDuration));
            SetOutLineColor(breakablePart, flashColor);
        }

        private void HandleCombatReset()
        {
            activeFlashes.Clear();

            foreach (var breakablePart in breakablePartManager.BreakableParts)
            {
                SetOutLineColor(breakablePart, Color.black);
            }
        }

        private void Update()
        {
            if (activeFlashes.Count == 0) return;
            for (int i = activeFlashes.Count - 1; i >= 0; i--)
            {
                ActiveFlash flash = activeFlashes[i];
                if (Time.time >= flash.EndTime)
                {
                    SetOutLineColor(flash.BreakablePart, idleColor);
                    activeFlashes.RemoveAt(i);
                    continue;
                }
                float remainingTime = flash.EndTime - Time.time;
                float progress = 1 - (remainingTime / flashDuration);
                SetOutLineColor(flash.BreakablePart, Color.Lerp(flashColor, idleColor, progress));
            }
        }
        private void SetOutLineColor(BreakablePart breakablePart, Color color)
        {
            propertyBlock.SetColor(BaseColorId, color);
            foreach (var renderer in breakablePart.Renderers)
            {
                if (renderer == null) continue;

                if (!outlineSlotByRenderer.TryGetValue(renderer, out int index)) continue;
                renderer.SetPropertyBlock(propertyBlock, index);
            }
        }

    }

}
