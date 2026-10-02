using System;
using TMPro;
using UnityEngine;

namespace ProjectFantasy.UI
{
    // 월드 공간 데미지 숫자: 떠오르며 사라짐, 항상 카메라를 향함 (SetText로 GC 없이 갱신)
    [RequireComponent(typeof(TextMeshPro))]
    [DisallowMultipleComponent]
    public sealed class DamageNumber : MonoBehaviour
    {
        private const string NumberFormat = "{0}";
        private const float StartProgress = 0f;
        private const float CompleteProgress = 1f;

        [SerializeField, Min(0.01f)] private float lifetime = 0.8f;
        [SerializeField] private float riseSpeed = 1.5f;
        [Tooltip("수명 비율(0~1)에 따른 투명도")]
        [SerializeField] private AnimationCurve alphaOverLifetime = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [Tooltip("수명 비율(0~1)에 따른 크기 배율")]
        [SerializeField] private AnimationCurve scaleOverLifetime = AnimationCurve.Constant(0f, 1f, 1f);

        private TextMeshPro text;
        private Transform cachedTransform;
        private Transform viewCamera;
        private Vector3 baseScale;
        private Color baseColor;
        private float elapsedTime;
        private Action<DamageNumber> onFinished;

        private void Awake()
        {
            text = GetComponent<TextMeshPro>();
            cachedTransform = transform;
            baseScale = cachedTransform.localScale;
        }

        public void Show(int amount, Vector3 position, Color color, Transform cameraTransform, Action<DamageNumber> finished)
        {
            text.SetText(NumberFormat, amount);
            baseColor = color;
            viewCamera = cameraTransform;
            onFinished = finished;
            elapsedTime = 0f;

            cachedTransform.position = position;
            ApplyLifetimeVisual(StartProgress);
            gameObject.SetActive(true);
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / lifetime;
            if (progress >= CompleteProgress)
            {
                gameObject.SetActive(false);
                onFinished?.Invoke(this);
                return;
            }

            cachedTransform.position += Vector3.up * (riseSpeed * Time.deltaTime);
            ApplyLifetimeVisual(progress);
        }

        private void LateUpdate()
        {
            if (viewCamera != null) cachedTransform.rotation = viewCamera.rotation;
        }

        private void ApplyLifetimeVisual(float progress)
        {
            Color color = baseColor;
            color.a = baseColor.a * alphaOverLifetime.Evaluate(progress);
            text.color = color;
            cachedTransform.localScale = baseScale * scaleOverLifetime.Evaluate(progress);
        }
    }
}
