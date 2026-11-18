using UnityEngine;
namespace ProjectFantasy.World
{
    // 목표·알림에 쓰는 장소 하나
    [CreateAssetMenu(fileName = "LocationData", menuName = "ProjectFantasy/World/Location Data")]
    public sealed class LocationData : ScriptableObject
    {
        [SerializeField] private string displayName;
        public string DisplayName => displayName;
    }
}
