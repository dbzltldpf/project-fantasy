using ProjectFantasy.Enemy;
using UnityEngine;
namespace ProjectFantasy.Audio
{
    // 보스 부위 파괴 시 그 부위 위치에서 파괴음 재생
    [RequireComponent(typeof(BreakablePartManager))]
    [DisallowMultipleComponent]
    public sealed class PartBreakSoundEmitter : MonoBehaviour
    {
        [SerializeField] private SoundPlayer soundPlayer;
        [SerializeField] private SoundData breakSound;

        private BreakablePartManager partManager;
        private void Awake()
        {
            partManager = GetComponent<BreakablePartManager>();
            if (soundPlayer == null) soundPlayer = FindFirstObjectByType<SoundPlayer>();
        }
        private void OnEnable()
        {
            partManager.PartBroken += HandlePartBroken;
        }
        private void OnDisable()
        {
            partManager.PartBroken -= HandlePartBroken;
        }
        private void HandlePartBroken(BreakablePart part) => soundPlayer.Play(breakSound, GetPartCenter(part));

        // 부위 히트박스 중심 (없으면 보스 위치)
        private Vector3 GetPartCenter(BreakablePart part) =>
            part.Colliders != null && part.Colliders.Length > 0 ? part.Colliders[0].bounds.center : transform.position;
    }
}
