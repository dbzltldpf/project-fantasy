using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFantasy.Combat
{
    // 이펙트 프리팹(파티클 등) 풀링, 재생 시간 후 자동 반환
    public sealed class EffectPool
    {
        public const float Infinite = float.PositiveInfinity;
        public const float OneShot = -1f;

        private readonly Dictionary<GameObject, Stack<PooledEffect>> pools = new Dictionary<GameObject, Stack<PooledEffect>>();
        private readonly Action<PooledEffect> returnAction;

        public EffectPool()
        {
            returnAction = Return;
        }

        // duration: OneShot(1회 재생 후 반환) / Infinite(Stop 호출 전까지 유지) / 초 단위 시간
        public PooledEffect Spawn(GameObject prefab, Vector3 position, Quaternion rotation, float duration)
        {
            if (prefab == null) return null;

            PooledEffect effect = Rent(prefab);
            effect.Play(position, rotation, duration, returnAction);
            return effect;
        }

        private PooledEffect Rent(GameObject prefab)
        {
            Stack<PooledEffect> pool = GetPool(prefab);
            while (pool.Count > 0)
            {
                PooledEffect pooled = pool.Pop();
                if (pooled != null) return pooled;
            }

            GameObject instance = Object.Instantiate(prefab);
            instance.SetActive(false);
            PooledEffect created = instance.AddComponent<PooledEffect>();
            created.Initialize(prefab);
            return created;
        }

        private void Return(PooledEffect effect) => GetPool(effect.SourcePrefab).Push(effect);

        private Stack<PooledEffect> GetPool(GameObject prefab)
        {
            if (!pools.TryGetValue(prefab, out Stack<PooledEffect> pool))
            {
                pool = new Stack<PooledEffect>();
                pools.Add(prefab, pool);
            }
            return pool;
        }
    }
}
