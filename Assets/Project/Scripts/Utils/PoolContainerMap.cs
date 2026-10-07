using System.Collections.Generic;
using UnityEngine;

namespace ProjectFantasy.Utils
{
    // 풀의 프리팹별 부모 폴더 캐시 (root 아래 프리팹 이름 폴더, root가 없으면 씬 최상위)
    public sealed class PoolContainerMap
    {
        private readonly Dictionary<Object, Transform> containers = new Dictionary<Object, Transform>();
        private Transform root;

        public void SetRoot(Transform poolRoot)
        {
            root = poolRoot;
            containers.Clear();
        }

        public Transform Get(Object prefab)
        {
            if (root == null) return null;
            if (containers.TryGetValue(prefab, out Transform container) && container != null) return container;

            container = PoolContainers.GetOrCreateChild(root, prefab.name);
            containers[prefab] = container;
            return container;
        }
    }
}
