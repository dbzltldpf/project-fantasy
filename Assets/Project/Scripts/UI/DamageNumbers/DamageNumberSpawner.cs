using System;
using ProjectFantasy.Utils;
using UnityEngine;

namespace ProjectFantasy.UI
{
    // 씬 단위 데미지 숫자 생성·풀링 (DamageNumberEmitter가 요청)
    [DisallowMultipleComponent]
    public sealed class DamageNumberSpawner : MonoBehaviour
    {
        [SerializeField] private DamageNumber damageNumberPrefab;
        [SerializeField] private Color damageColor = Color.white;
        [Tooltip("구역 밖 공격 등 피해 거부 시 표시할 글자")]
        [SerializeField] private string immuneText = "무효";
        [SerializeField] private Color immuneColor = Color.green;
        [SerializeField] private Transform cameraTransform;
        [Tooltip("타격 지점 기준 생성 오프셋")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.3f, 0f);
        [Tooltip("숫자가 겹치지 않도록 수평 랜덤 오프셋 반경")]
        [SerializeField, Min(0f)] private float randomHorizontalRadius = 0.3f;
        [Tooltip("데미지 숫자 부모 (비우면 Pools/DamageNumbers 아래 프리팹별 폴더)")]
        [SerializeField] private Transform poolRoot;

        private readonly PrefabPool<DamageNumber> pool = new PrefabPool<DamageNumber>();
        private Action<DamageNumber> returnAction;

        private void Awake()
        {
            returnAction = ReturnToPool;
            pool.SetRoot(PoolContainers.Resolve(poolRoot, PoolContainers.DamageNumbers));
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        }

        public void Spawn(int amount, Vector3 worldPosition)
        {
            if (damageNumberPrefab == null) return;
            Rent(worldPosition, out Vector3 position).Show(amount, position, damageColor, cameraTransform, returnAction);
        }
        public void SpawnImmune(Vector3 worldPosition)
        {
            if (damageNumberPrefab == null) return;
            Rent(worldPosition, out Vector3 position).ShowText(immuneText, position, immuneColor, cameraTransform, returnAction);
        }
        private DamageNumber Rent(Vector3 worldPosition, out Vector3 position)
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * randomHorizontalRadius;
            position = worldPosition + spawnOffset + new Vector3(randomOffset.x, 0f, randomOffset.y);
            return pool.Rent(damageNumberPrefab);
        }
        private void ReturnToPool(DamageNumber number) => pool.Return(damageNumberPrefab, number);
    }
}
