using System.Collections.Generic;
using UnityEngine;

namespace ProjectFantasy.Utils
{
    // 프리팹별 컴포넌트 풀 (외부에서 파괴된 인스턴스는 건너뜀)
    public sealed class PrefabPool<T> where T : Component
    {
        private readonly Dictionary<T, Stack<T>> pools = new Dictionary<T, Stack<T>>();

        public T Rent(T prefab)
        {
            Stack<T> pool = GetPool(prefab);
            while (pool.Count > 0)
            {
                T pooled = pool.Pop();
                if (pooled != null) return pooled;
            }

            return Object.Instantiate(prefab);
        }

        public void Return(T prefab, T instance)
        {
            instance.gameObject.SetActive(false);
            GetPool(prefab).Push(instance);
        }

        private Stack<T> GetPool(T prefab)
        {
            if (!pools.TryGetValue(prefab, out Stack<T> pool))
            {
                pool = new Stack<T>();
                pools.Add(prefab, pool);
            }
            return pool;
        }
    }
}
