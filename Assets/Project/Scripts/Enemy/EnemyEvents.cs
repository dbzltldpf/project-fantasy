using System;

namespace ProjectFantasy.Enemy
{
    // 적 관련 전역 알림
    public static class EnemyEvents
    {
        public static event Action<EnemyData> EnemyKilled;
        public static void RaiseEnemyKilled(EnemyData enemyData)
        {
            EnemyKilled?.Invoke(enemyData);
        }
    }

}
