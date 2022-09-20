using UnityEngine;

namespace Utils
{
    public class HoverInteractable : MonoBehaviour
    {
        [SerializeField]
        private Material mat;

        private static readonly int Hovered = Shader.PropertyToID("_hovered");

        public void Hover()
        {
            mat.SetInt(Hovered, 1);
        }

        public void HoverExit()
        {
            mat.SetInt(Hovered, 0);
        }
    }
}
