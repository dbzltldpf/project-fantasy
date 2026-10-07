using System.Collections.Generic;
using ProjectFantasy.Combat;
using ProjectFantasy.Items;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 사망 시 드랍 표를 굴려 주변에 필드 아이템 생성
    [RequireComponent(typeof(EnemyController))]
    [DisallowMultipleComponent]
    public sealed class EnemyLoot : MonoBehaviour
    {
        private const int InitialDropCapacity = 4;
        private const float FullTurnDegrees = 360f;

        [Tooltip("필드 아이템 프리팹 (플레이어 버리기와 같은 WorldItem)")]
        [SerializeField] private WorldItem worldItemPrefab;
        [Tooltip("드랍 위치 흩어짐 반경 (m)")]
        [SerializeField, Min(0f)] private float scatterRadius = 1f;
        [Tooltip("드랍 높이 (m, 지면 아래로 묻히지 않게)")]
        [SerializeField, Min(0f)] private float dropHeight = 0.3f;

        private readonly List<LootDrop> drops = new List<LootDrop>(InitialDropCapacity);
        private EnemyController controller;
        private Health health;

        private void Awake()
        {
            controller = GetComponent<EnemyController>();
            health = GetComponent<Health>();
        }

        private void OnEnable() => health.Died += HandleDied;
        private void OnDisable() => health.Died -= HandleDied;

        private void HandleDied()
        {
            DropTable table = controller.Data.DropTable;
            if (table == null || worldItemPrefab == null) return;

            drops.Clear();
            table.Roll(drops);

            foreach (LootDrop drop in drops)
            {
                Vector2 offset = Random.insideUnitCircle * scatterRadius;
                Vector3 position = transform.position + new Vector3(offset.x, dropHeight, offset.y);
                WorldItem.Spawn(worldItemPrefab, drop, position, Quaternion.Euler(0f, Random.Range(0f, FullTurnDegrees), 0f));
            }
        }
    }
}
