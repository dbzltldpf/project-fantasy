using System;
using UnityEngine;

namespace ProjectFantasy.Items
{
    // 소모품 효과 베이스 (에셋 공유 데이터, 사용자에게 1회 적용)
    [Serializable]
    public abstract class ConsumableEffect
    {
        public abstract void Apply(GameObject user);
    }
}
