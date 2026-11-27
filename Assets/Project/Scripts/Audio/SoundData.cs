using UnityEngine;
namespace ProjectFantasy.Audio
{
    // 소리 하나의 설정 (클립 무작위·피치 변화로 반복감 줄임)
    [CreateAssetMenu(fileName = "SoundData", menuName = "ProjectFantasy/Sound/Sound Data")]
    public sealed class SoundData : ScriptableObject
    {
        [Tooltip("재생할 클립 (여러 개면 무작위)")]
        [SerializeField] private AudioClip[] clips;
        [SerializeField, Range(0, 1)] private float volume = 1f;
        [SerializeField] private float minPitch = 0.95f;
        [SerializeField] private float maxPitch = 1.05f;
        [Tooltip("0 = 2D (UI·BGM), 1 = 3D (위치에 따라 감쇠)")]
        [SerializeField, Range(0, 1)] private float spatialBlend = 1;
        public float Volume => volume;
        public float SpatialBlend => spatialBlend;
        public AudioClip PickClip() => clips == null || clips.Length == 0 ? null : clips[Random.Range(0, clips.Length)];
        public float PickPitch() => Random.Range(minPitch, maxPitch);
    }
}
