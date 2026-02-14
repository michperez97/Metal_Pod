using UnityEngine;

namespace MetalPod.Workshop
{
    public class CustomizationSystem : MonoBehaviour
    {
        [SerializeField] private Renderer hovercraftRenderer;
        [SerializeField] private string decalMaterialProperty = "_DecalId";
        [SerializeField] private string partMaterialProperty = "_PartId";

        public void ApplyColor(Color color)
        {
            if (hovercraftRenderer == null)
            {
                return;
            }

            hovercraftRenderer.material.color = color;
        }

        public void ApplyDecal(string decalId)
        {
            if (hovercraftRenderer == null || string.IsNullOrEmpty(decalId))
            {
                return;
            }

            hovercraftRenderer.material.SetFloat(decalMaterialProperty, Animator.StringToHash(decalId));
        }

        public void ApplyPart(string partId)
        {
            if (hovercraftRenderer == null || string.IsNullOrEmpty(partId))
            {
                return;
            }

            hovercraftRenderer.material.SetFloat(partMaterialProperty, Animator.StringToHash(partId));
        }
    }
}
