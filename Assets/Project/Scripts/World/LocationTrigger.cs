using UnityEngine;
namespace ProjectFantasy.World
{
    // 대상 레이어가 처음 들어오면 장소 도착 알림
    [RequireComponent(typeof(Collider))]
    public sealed class LocationTrigger : MonoBehaviour
    {
        [Tooltip("도착을 알릴 장소")]
        [SerializeField] private LocationData locationData;
        [Tooltip("도착으로 인정할 레이어")]
        [SerializeField] private LayerMask targetLayers;
        private bool hasReached;
        private void OnTriggerEnter(Collider other)
        {
            if (hasReached) return;
            if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
            hasReached = true;
            LocationEvents.RaiseReached(locationData);
        }
    }

}
