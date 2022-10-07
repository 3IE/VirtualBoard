using System.Linq;
using UnityEngine;

namespace Board.Tools
{
    /// <summary>
    /// Singleton class, holds a list of parameters for the tools
    /// </summary>
    public class Tools : MonoBehaviour
    {
        public static Tools Instance { get; private set; }
        
        [Tooltip(
            "How much we want to interpolate the draw between two frames (lower values means more data is covered)")]
        [Range(0.01f, 1.00f)]
        public float coverage = 0.01f;

        public Marker marker;
        public Eraser eraser;

        public Color baseColor;

        private void Awake()
        {
            Instance = this;
        }

        public static Color[] GenerateSquare(Color color, float penSize)
        {
            return Enumerable.Repeat(color, (int)(penSize * penSize)).ToArray();
        }
    }
}