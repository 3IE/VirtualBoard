using System.Linq;
using UnityEngine;

namespace Board.Tools
{
    /// <summary>
    /// Singleton class, holds a list of parameters for the tools
    /// </summary>
    public class Tools : MonoBehaviour
    {
        /// <summary>
        /// Instance of this class
        /// </summary>
        public static Tools Instance { get; private set; }
        
        /// <summary>
        /// How much we want to interpolate the draw between two frames (lower values means more data is covered)
        /// </summary>
        [Tooltip(
            "How much we want to interpolate the draw between two frames (lower values means more data is covered)")]
        [Range(0.01f, 1.00f)]
        public float coverage = 0.01f;

        /// <summary>
        /// Instance of the marker
        /// </summary>
        public Marker marker;
        /// <summary>
        /// Instance of the eraser
        /// </summary>
        public Eraser eraser;

        /// <summary>
        /// Base color of the board
        /// </summary>
        public Color baseColor;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Generates an array of colors
        /// </summary>
        /// <param name="color"> color used to populate the array </param>
        /// <param name="penSize"> size of the array </param>
        /// <returns> the array created </returns>
        public static Color[] GenerateSquare(Color color, float penSize)
        {
            return Enumerable.Repeat(color, (int)(penSize * penSize)).ToArray();
        }
    }
}