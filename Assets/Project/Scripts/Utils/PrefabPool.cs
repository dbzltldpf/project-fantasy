using System.Collections.Generic;
using UnityEngine;

namespace ProjectFantasy.Utils
{
    // 프리팹별 컴포넌트 풀 (외부에서 파괴된 인스턴스는 건너뜀), 부모를 지정하면 그 아래 프리팹별 폴더에 생성
    public sealed class PrefabPool<T> where T : Component
    {
        private readonly Dictionary<T, Stack<T>> pools = new Dictionary<T, Stack<T>>();
        private readonly PoolContainerMap containers = new PoolContainerMap();

        // 필드 초기화 시점에는 씬 오브젝트를 만들 수 없어 Awake에서 호출
        public void SetRoot(Transform poolRoot) => containers.SetRoot(poolRoot);

        public T Rent(T prefab)
        {
            Stack<T> pool = GetPool(prefab);
            while (pool.Count > 0)
            {
                T pooled = pool.Pop();
                if (pooled != null) return pooled;
            }

            return Object.Instantiate(prefab, containers.Get(prefab));
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
