using UnityEngine;
using UnityEngine.Audio;
namespace ProjectFantasy.Audio
{
    // AudioSource 풀을 돌려 쓰며 위치에 소리 재생 (씬에 하나)
    public sealed class SoundPlayer : MonoBehaviour
    {
        private const string SourceName = "SoundSource";
        [Tooltip("동시에 낼 수 있는 소리 수 (다 차면 가장 오래된 소리를 덮어씀)")]
        [SerializeField, Min(1)] private int poolSize = 16;
        [SerializeField] private AudioMixerGroup output;
        [Tooltip("이 거리까지 최대 볼륨")]
        [SerializeField, Min(0)] private float minDistance = 2f;
        [Tooltip("이 거리에서 0 (m)")]
        [SerializeField, Min(0)] private float maxDistance = 30f;
        private AudioSource[] sources;
        private int nextIndex;
        private void Awake()
        {
            sources = new AudioSource[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                AudioSource source = new GameObject(SourceName).AddComponent<AudioSource>();
                source.transform.SetParent(transform);
                source.playOnAwake = false;
                source.outputAudioMixerGroup = output;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = minDistance;
                source.maxDistance = maxDistance;
                sources[i] = source;
            }
        }
        public void Play(SoundData sound, Vector3 position)
        {
            AudioClip clip = sound != null ? sound.PickClip() : null;
            if (clip == null) return;
            AudioSource source = sources[nextIndex];
            nextIndex = (nextIndex + 1) % sources.Length;
            source.transform.position = position;
            source.clip = clip;
            source.volume = sound.Volume;
            source.pitch = sound.PickPitch();
            source.spatialBlend = sound.SpatialBlend;
            source.Play();
        }
    }
}
