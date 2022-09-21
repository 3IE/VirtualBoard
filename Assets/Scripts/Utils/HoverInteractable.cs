using System;
using UnityEngine;

namespace Utils
{
    public class HoverInteractable : MonoBehaviour
    {
        private Material _mat;

        private static readonly int Hovered = Shader.PropertyToID("_hovered");

        private void Start()
        {
            _mat = GetComponent<Renderer>().material;
        }

        public void Hover()
        {
            _mat.SetInt(Hovered, 1);
        }

        public void HoverExit()
        {
            _mat.SetInt(Hovered, 0);
        }
    }
}
