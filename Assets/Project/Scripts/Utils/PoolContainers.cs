using UnityEngine;

namespace ProjectFantasy.Utils
{
    // 풀 인스턴스 정리용 씬 폴더 ("Pools/분류/프리팹 이름"), 없으면 생성
    public static class PoolContainers
    {
        public const string Projectiles = "Projectiles";
        public const string Effects = "Effects";
        public const string DamageNumbers = "DamageNumbers";

        private const string RootName = "Pools";

        // 지정한 부모가 있으면 그대로, 없으면 Pools/분류 폴더
        public static Transform Resolve(Transform overrideRoot, string category)
        {
            return overrideRoot != null ? overrideRoot : GetCategory(category);
        }

        public static Transform GetCategory(string category)
        {
            GameObject root = GameObject.Find(RootName);
            if (root == null) root = new GameObject(RootName);
            return GetOrCreateChild(root.transform, category);
        }

        public static Transform GetOrCreateChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null) return child;

            child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }
    }
}
