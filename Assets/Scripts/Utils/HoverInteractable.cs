using UnityEngine;

namespace Utils
{
    /// <summary>
    ///     Utility class used for object we want to highlight when a controller hovers over them
    /// </summary>
    public class HoverInteractable : MonoBehaviour
    {
        private static readonly int      Hovered = Shader.PropertyToID("_hovered");
        private                 Material _mat;

        private void Start()
        {
            _mat = GetComponent<Renderer>().material;
        }

        /// <summary>
        ///     Called when a controller hovers over this object
        /// </summary>
        public void Hover()
        {
            _mat.SetInt(Hovered, 1);
        }

        /// <summary>
        ///     Called when a controller stops hovering over this object
        /// </summary>
        public void HoverExit()
        {
            _mat.SetInt(Hovered, 0);
        }
    }
}