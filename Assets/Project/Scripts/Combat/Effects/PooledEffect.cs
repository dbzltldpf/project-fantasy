using System;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 풀링 이펙트 인스턴스: 1회 재생(파티클 종료 시 반환) / 시간 지정 / 수동 종료 (EffectPool이 자동 부착)
    [DisallowMultipleComponent]
    public sealed class PooledEffect : MonoBehaviour
    {
        // 1회 재생 프리팹의 루프가 켜져 있을 때 강제 반환 시간
        private const float MaxOneShotLifetime = 5f;

        private ParticleSystem[] particleSystems;
        private Action<PooledEffect> onFinished;
        private float remainingTime;
        private bool isOneShot;

        public GameObject SourcePrefab { get; private set; }

        public void Initialize(GameObject sourcePrefab)
        {
            SourcePrefab = sourcePrefab;
            particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }

        // duration: EffectPool.OneShot(1회 재생) / EffectPool.Infinite(Stop까지 유지) / 초 단위 시간
        public void Play(Vector3 position, Quaternion rotation, float duration, Action<PooledEffect> finished)
        {
            isOneShot = duration < 0f;
            remainingTime = isOneShot ? MaxOneShotLifetime : duration;
            onFinished = finished;

            transform.SetPositionAndRotation(position, rotation);
            gameObject.SetActive(true);
        }

        public void Stop()
        {
            if (!gameObject.activeSelf) return;

            gameObject.SetActive(false);
            onFinished?.Invoke(this);
        }

        public void MoveTo(Vector3 position) => transform.position = position;

        private void Update()
        {
            if (isOneShot && !IsAnyParticleAlive())
            {
                Stop();
                return;
            }

            if (float.IsPositiveInfinity(remainingTime)) return;

            remainingTime -= Time.deltaTime;
            if (remainingTime > 0f) return;

            if (isOneShot) WarnLoopingOneShot();
            Stop();
        }

        private bool IsAnyParticleAlive()
        {
            for (int i = 0; i < particleSystems.Length; i++)
            {
                if (particleSystems[i].IsAlive(false)) return true;
            }
            return false;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void WarnLoopingOneShot()
        {
            Debug.LogWarning($"[{nameof(PooledEffect)}] '{SourcePrefab.name}'가 {MaxOneShotLifetime}초 안에 끝나지 않아 강제 반환했습니다. 1회 재생용 Prefab Variant의 Looping을 끄세요.", SourcePrefab);
        }
    }
}
