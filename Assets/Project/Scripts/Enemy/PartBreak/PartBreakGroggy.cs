using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 부위 파괴 시 보스에게 그로기 요청
    [RequireComponent(typeof(BreakablePartManager), typeof(EnemyController))]
    [DisallowMultipleComponent]
    public sealed class PartBreakGroggy : MonoBehaviour
    {
        private EnemyController enemyController;
        private BreakablePartManager breakablePartManager;
        [Tooltip("그로기 시간 (초)")]
        [SerializeField, Min(0)] private float groggyDuration = 3f;
        private void Awake()
        {
            enemyController = GetComponent<EnemyController>();
            breakablePartManager = GetComponent<BreakablePartManager>();
        }
        private void OnEnable()
        {
            breakablePartManager.PartBroken += HandlePartBroken;
        }
        private void OnDisable()
        {
            breakablePartManager.PartBroken -= HandlePartBroken;
        }
        private void HandlePartBroken(BreakablePart breakablePart)
        {
            enemyController.StartGroggy(groggyDuration);
        }
    }

}
