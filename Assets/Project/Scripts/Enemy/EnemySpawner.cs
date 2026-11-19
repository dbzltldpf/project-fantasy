using System.Collections.Generic;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 스폰 지점: 반경 안에 정해진 수만큼 생성, 사망·비활성화된 적은 대기 후 같은 인스턴스로 리스폰 (풀링)
    [DisallowMultipleComponent]
    public sealed class EnemySpawner : MonoBehaviour
    {
        private const float FullTurnDegrees = 360f;

        [Tooltip("생성할 적 프리팹")]
        [SerializeField] private EnemyController enemyPrefab;
        [Tooltip("동시에 유지할 마리 수")]
        [SerializeField, Min(1)] private int count = 3;
        [Tooltip("스폰 반경 (m, NavMesh 위 무작위 지점)")]
        [SerializeField, Min(0f)] private float spawnRadius = 5f;
        [Tooltip("사라진 뒤 다시 나타나기까지 시간 (초)")]
        [SerializeField, Min(0f)] private float respawnDelay = 30f;
        [Tooltip("생성한 적이 사용할 진영 영역 (비우면 Leash Range 원)")]
        [SerializeField] private EnemyTerritory territory;
        [Tooltip("선택 시 스폰 반경 Gizmo 색")]
        [SerializeField] private Color gizmoColor = new Color(1f, 0.3f, 0.3f, 0.5f);
        [Tooltip("죽은 적을 다시 살림 (스테이지 캠프는 끔)")]
        [SerializeField] private bool respawn = false;
        private readonly List<PendingRespawn> pendingRespawns = new List<PendingRespawn>();

        private readonly struct PendingRespawn
        {
            public readonly EnemyController Enemy;
            public readonly float Time;

            public PendingRespawn(EnemyController enemy, float time)
            {
                Enemy = enemy;
                Time = time;
            }
        }

        private void Start()
        {
            if (enemyPrefab == null)
            {
                Debug.LogError($"[{name}] Enemy Prefab이 비어 있습니다.", this);
                return;
            }

            for (int i = 0; i < count; i++)
            {
                Vector3 position = GetSpawnPoint();
                EnemyController enemy = Instantiate(enemyPrefab, position, GetRandomRotation(), transform);
                enemy.SetTerritory(territory);
                enemy.Despawned += HandleDespawned;
                enemy.Spawn(position, enemy.transform.rotation);
            }
        }

        // 역순 순회로 제거해도 인덱스 유지
        private void Update()
        {
            for (int i = pendingRespawns.Count - 1; i >= 0; i--)
            {
                PendingRespawn pending = pendingRespawns[i];
                if (Time.time < pending.Time) continue;

                pendingRespawns.RemoveAt(i);
                Respawn(pending.Enemy);
            }
        }

        private void OnDestroy()
        {
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent(out EnemyController enemy)) enemy.Despawned -= HandleDespawned;
            }
        }

        private void HandleDespawned(EnemyController enemy)
        {
            if (!respawn) return;
            pendingRespawns.Add(new PendingRespawn(enemy, Time.time + respawnDelay));
        }

        private void Respawn(EnemyController enemy)
        {
            if (enemy == null) return;

            enemy.gameObject.SetActive(true);
            enemy.Spawn(GetSpawnPoint(), GetRandomRotation());
        }
        // 대기 중인 리스폰을 취소하고 자식 적 전부를 껐다 켜서 처음 상태로 다시 스폰 (스테이지 재시작)
        public void ResetEnemies()
        {
            pendingRespawns.Clear();
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent(out EnemyController enemy))
                {
                    enemy.gameObject.SetActive(false);
                    Respawn(enemy);
                }
            }
        }
        private Vector3 GetSpawnPoint()
        {
            EnemyNavigator.TryGetRandomPoint(transform.position, spawnRadius, out Vector3 point);
            return point;
        }

        private static Quaternion GetRandomRotation() => Quaternion.Euler(0f, Random.Range(0f, FullTurnDegrees), 0f);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
