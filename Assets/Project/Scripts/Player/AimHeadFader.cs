using UnityEngine;
namespace ProjectFantasy.Player
{
    // 조준 모드에서 머리 메시를 반투명 머티리얼로 교체 (화면 가림 방지)
    public sealed class AimHeadFader : MonoBehaviour
    {
        [SerializeField] private Renderer[] headRenderers;
        private Material[] originalMaterials;
        [SerializeField] private Material fadeMaterial;
        private PlayerController playerController;
        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            originalMaterials = new Material[headRenderers.Length];
            for (int i = 0; i < headRenderers.Length; i++)
            {
                originalMaterials[i] = headRenderers[i].sharedMaterial;
            }
        }
        private void OnEnable()
        {
            playerController.AimViewChanged += SetFaded;
        }
        private void OnDisable()
        {
            playerController.AimViewChanged -= SetFaded;
            SetFaded(false);
        }
        private void SetFaded(bool isFaded)
        {
            for (int i = 0; i < headRenderers.Length; i++)
            {
                headRenderers[i].sharedMaterial = isFaded ? fadeMaterial : originalMaterials[i];
            }
        }
    }
}
