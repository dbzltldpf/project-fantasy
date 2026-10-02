using System;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 칼날 위 샘플 지점의 이전/현재 월드 위치 추적 (이전 → 현재 구간을 캡슐로 쓸어 판정)
    public sealed class WeaponTrace
    {
        private const int MinSampleCount = 2;
        private const int MaxSampleCount = 8;
        private const float DiameterPerRadius = 2f;

        private readonly Vector3[] localPoints = new Vector3[MaxSampleCount];
        private Vector3[] previousPoints = new Vector3[MaxSampleCount];
        private Vector3[] currentPoints = new Vector3[MaxSampleCount];
        private Transform blade;

        public bool IsAttached => blade != null;
        public int SampleCount { get; private set; }
        public float Radius { get; private set; }

        // 샘플 간격이 판정 지름을 넘지 않도록 개수 결정
        public void Attach(Transform bladeTransform, Vector3 localBase, Vector3 localTip, float radius)
        {
            blade = bladeTransform;
            Radius = radius;

            float spacing = radius * DiameterPerRadius;
            int count = Mathf.CeilToInt(Vector3.Distance(localBase, localTip) / spacing) + 1;
            SampleCount = Mathf.Clamp(count, MinSampleCount, MaxSampleCount);

            int lastIndex = SampleCount - 1;
            for (int i = 0; i < SampleCount; i++)
            {
                localPoints[i] = Vector3.Lerp(localBase, localTip, (float)i / lastIndex);
            }
        }

        public void Detach() => blade = null;

        // 스윙 시작: 이전 위치를 현재 위치로 초기화
        public void Begin()
        {
            Sample(currentPoints);
            Array.Copy(currentPoints, previousPoints, SampleCount);
        }

        // 판정 직전 1회: 현재 → 이전으로 넘기고 새 위치 샘플링
        public void Advance()
        {
            (previousPoints, currentPoints) = (currentPoints, previousPoints);
            Sample(currentPoints);
        }

        public Vector3 GetPrevious(int index) => previousPoints[index];
        public Vector3 GetCurrent(int index) => currentPoints[index];
        public Vector3 GetWorldPoint(int index) => blade.TransformPoint(localPoints[index]);

        private void Sample(Vector3[] buffer)
        {
            for (int i = 0; i < SampleCount; i++)
            {
                buffer[i] = blade.TransformPoint(localPoints[i]);
            }
        }
    }
}
