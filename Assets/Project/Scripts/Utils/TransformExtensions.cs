using UnityEngine;

namespace ProjectFantasy.Utils
{
    public static class TransformExtensions
    {
        // 하위 계층 전체에서 이름으로 탐색 (초기화 시점 전용)
        public static Transform FindDeepChild(this Transform parent, string childName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == childName) return child;

                Transform found = child.FindDeepChild(childName);
                if (found != null) return found;
            }
            return null;
        }
    }
}
