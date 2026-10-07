using UnityEngine;

namespace ProjectFantasy.Utils
{
    // 모델 렌더러 머티리얼 일괄 교체 (생성 시 1회, sharedMaterial 사용으로 인스턴스 복제 없음)
    public static class RendererExtensions
    {
        public static void ApplySharedMaterial(this GameObject root, Material material)
        {
            if (material == null) return;

            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = material;
                }
                renderer.sharedMaterials = materials;
            }
        }
    }
}
