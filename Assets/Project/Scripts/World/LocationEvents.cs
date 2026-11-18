using System;

namespace ProjectFantasy.World
{
    // 장소 관련 전역 알림
    public static class LocationEvents
    {
        public static event Action<LocationData> Reached;
        public static void RaiseReached(LocationData locationData)
        {
            Reached?.Invoke(locationData);
        }

    }
}
