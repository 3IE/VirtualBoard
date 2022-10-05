using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Board.Tools
{
    public class Tools : MonoBehaviour
    {
        [Tooltip(
            "How much we want to interpolate the draw between two frames (lower values means more data is covered)")]
        [Range(0.01f, 1.00f)]
        public float coverage = 0.01f;

        public Marker marker;
        public Eraser eraser;

        public Color baseColor;

        public static Color[] GenerateSquare(Color color, float penSize)
        {
            return Enumerable.Repeat(color, (int)(penSize * penSize)).ToArray();
        }
    }
}