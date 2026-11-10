using UnityEngine;
using ProjectFantasy.Core;

namespace ProjectFantasy.Enemy
{
    // 무기별 경직 횟수·면역 판정 (제한 없는 공격은 항상 경직)
    public sealed class EnemyStaggerCounter
    {
        private int staggerCount;
        private float windowEndTime;
        public bool TryStagger(int limit, float windowDuration)
        {
            if (limit == DamageInfo.NoStaggerLimit) return true;
            if (windowEndTime < Time.time) staggerCount = 0;
            if (staggerCount >= limit) return false;
            staggerCount++;
            windowEndTime = Time.time + windowDuration;
            return true;
        }
        public void Reset()
        {
            staggerCount = 0;
            windowEndTime = 0f;
        }
    }

}
