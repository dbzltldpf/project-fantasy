using System.Collections.Generic;
using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using ProjectFantasy.Utils;
using UnityEngine;

namespace ProjectFantasy.Magic
{
    // 범위 마법 처리: 위치 지정 마법진 표시, 시전 후 지연 발동, 범위 피해 (플레이어/적 공용)
    [DisallowMultipleComponent]
    public sealed class SpellCaster : MonoBehaviour
    {
        private const int MaxAreaTargets = 32;
        private const int InitialPendingCapacity = 8;

        [Tooltip("범위 피해 대상 레이어 (Player 제외)")]
        [SerializeField] private LayerMask areaDamageLayers = Physics.DefaultRaycastLayers;
        [Tooltip("마법진 위치 지정용 지면 레이어 (캐릭터·투사체 제외)")]
        [SerializeField] private LayerMask groundLayers = Physics.DefaultRaycastLayers;
        [Tooltip("마법진 조준 레이 최대 거리 (m)")]
        [SerializeField, Min(0f)] private float maxTargetingRayDistance = 100f;
        [Tooltip("사거리 밖 지점을 땅에 붙일 때 위에서 쏘는 높이 (m)")]
        [SerializeField, Min(0f)] private float groundProbeHeight = 10f;
        [Tooltip("마법진·마법 이펙트 부모 (비우면 Pools/Effects 아래 프리팹별 폴더)")]
        [SerializeField] private Transform effectRoot;

        private readonly EffectPool effectPool = new EffectPool();
        private readonly Collider[] overlapBuffer = new Collider[MaxAreaTargets];
        private readonly HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();
        private readonly List<PendingArea> pendingAreas = new List<PendingArea>(InitialPendingCapacity);

        private IDamageable owner;
        private PooledEffect activeIndicator;

        private readonly struct PendingArea
        {
            public readonly AreaSpellData Spell;
            public readonly Vector3 Position;
            public readonly int Damage;
            public readonly float ActivateTime;

            public PendingArea(AreaSpellData spell, Vector3 position, int damage, float activateTime)
            {
                Spell = spell;
                Position = position;
                Damage = damage;
                ActivateTime = activateTime;
            }
        }

        private void Awake()
        {
            owner = GetComponent<IDamageable>();
            effectPool.SetRoot(PoolContainers.Resolve(effectRoot, PoolContainers.Effects));
        }

        private void OnDisable()
        {
            HideIndicator();
        }

        private void Update()
        {
            float now = Time.time;
            for (int i = pendingAreas.Count - 1; i >= 0; i--)
            {
                if (now < pendingAreas[i].ActivateTime) continue;

                Activate(pendingAreas[i]);
                pendingAreas.RemoveAt(i);
            }
        }

        public bool TryGetTargetPoint(Ray aimRay, float maxRange, out Vector3 point)
        {
            return GroundTargeting.TryResolve(transform.position, aimRay, maxRange, maxTargetingRayDistance, groundProbeHeight, groundLayers, out point);
        }

        // 조준 중 마법진이 지정 위치를 따라다님
        public void ShowIndicator(AreaSpellData spell, Vector3 position)
        {
            if (activeIndicator != null)
            {
                activeIndicator.MoveTo(position);
                return;
            }

            activeIndicator = effectPool.Spawn(spell.IndicatorPrefab, position, Quaternion.identity, EffectPool.Infinite);
            ApplyIndicatorScale(activeIndicator, spell);
        }

        public void HideIndicator()
        {
            if (activeIndicator == null) return;

            activeIndicator.Stop();
            activeIndicator = null;
        }

        // 시전 위치에 마법진을 남기고 지연 후 발동
        public void CastArea(AreaSpellData spell, Vector3 position, int damage)
        {
            PooledEffect circle = effectPool.Spawn(spell.IndicatorPrefab, position, Quaternion.identity, spell.ActivationDelay);
            ApplyIndicatorScale(circle, spell);
            pendingAreas.Add(new PendingArea(spell, position, damage, Time.time + spell.ActivationDelay));
        }

        private void Activate(in PendingArea area)
        {
            effectPool.Spawn(area.Spell.AreaEffectPrefab, area.Position, Quaternion.identity, EffectPool.OneShot);

            int hitCount = Physics.OverlapSphereNonAlloc(area.Position, area.Spell.Radius, overlapBuffer, areaDamageLayers, QueryTriggerInteraction.Ignore);
            damagedTargets.Clear();

            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = overlapBuffer[i];
                IDamageable target = hitCollider.GetComponentInParent<IDamageable>();
                if (target == null || target == owner || !target.IsAlive || !damagedTargets.Add(target)) continue;

                Vector3 direction = hitCollider.transform.position - area.Position;
                direction.y = 0f;
                Vector3 knockbackDirection = direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : transform.forward;
                target.TakeDamage(new DamageInfo(area.Damage, hitCollider.ClosestPointOnBounds(area.Position), knockbackDirection, gameObject, DamageType.Magic));
            }
        }

        private static void ApplyIndicatorScale(PooledEffect indicator, AreaSpellData spell)
        {
            if (indicator == null) return;
            indicator.transform.localScale = spell.IndicatorPrefab.transform.localScale * spell.IndicatorScale;
        }
    }
}
