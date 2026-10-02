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
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Color damageColor = Color.white;
        [Tooltip("타격 지점 기준 생성 오프셋")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.3f, 0f);
        [Tooltip("숫자가 겹치지 않도록 수평 랜덤 오프셋 반경")]
        [SerializeField, Min(0f)] private float randomHorizontalRadius = 0.3f;

        private readonly PrefabPool<DamageNumber> pool = new PrefabPool<DamageNumber>();
        private Action<DamageNumber> returnAction;

        private void Awake()
        {
            returnAction = ReturnToPool;
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        }

        public void Spawn(int amount, Vector3 worldPosition)
        {
            if (damageNumberPrefab == null) return;

            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * randomHorizontalRadius;
            Vector3 position = worldPosition + spawnOffset + new Vector3(randomOffset.x, 0f, randomOffset.y);

            DamageNumber number = pool.Rent(damageNumberPrefab);
            number.Show(amount, position, damageColor, cameraTransform, returnAction);
        }

        private void ReturnToPool(DamageNumber number) => pool.Return(damageNumberPrefab, number);
    }
}
