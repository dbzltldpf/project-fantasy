using UnityEngine;
namespace ProjectFantasy.Combat
{
    // 화살이 박힐 뼈 찾기: 맞은 지점에서 가장 가까운 뼈 (사망 모션을 따라가게)
    [DisallowMultipleComponent]
    public sealed class ProjectileStickBones : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer skinnedMesh;
        private Transform[] bones;
        private void Awake()
        {
            if (skinnedMesh == null) skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
            bones = skinnedMesh.bones;
        }
        public Transform FindNearest(Vector3 point)
        {
            Transform nearest = null;
            float nearestSqr = float.MaxValue;
            foreach (var bone in bones)
            {
                float sqr = (bone.position - point).sqrMagnitude;
                if (sqr < nearestSqr)
                {
                    nearestSqr = sqr;
                    nearest = bone;
                }
            }
            return nearest;
        }
    }
}
