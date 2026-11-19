using UnityEngine;
namespace ProjectFantasy.World
{
    // 대상 레이어가 들어오면 장소 도착 알림 (기본 1회)
    [RequireComponent(typeof(Collider))]
    public sealed class LocationTrigger : MonoBehaviour
    {
        [Tooltip("도착을 알릴 장소")]
        [SerializeField] private LocationData locationData;
        [Tooltip("도착으로 인정할 레이어")]
        [SerializeField] private LayerMask targetLayers;
        [Tooltip("처음 한 번만 알림 (끄면 들어올 때마다 알림)")]
        [SerializeField] private bool triggerOnce = true;
        private bool hasReached;
        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnce && hasReached) return;
            if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
            hasReached = true;
            LocationEvents.RaiseReached(locationData);
        }
    }

}
