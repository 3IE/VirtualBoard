using System.Linq;
using UnityEngine;

namespace Board.Tools
{
    /// <summary>
    ///     Singleton class, holds a list of parameters for the tools
    /// </summary>
    public class Tools : MonoBehaviour
    {
        /// <summary>
        ///     How much we want to interpolate the draw between two frames (lower values means more data is covered)
        /// </summary>
        [Tooltip(
                    "How much we want to interpolate the draw between two frames (lower values means more data is covered)")]
        [Range(0.01f, 1.00f)]
        public float coverage = 0.01f;

        /// <summary>
        ///     Instance of the marker
        /// </summary>
        public Marker marker;

        /// <summary>
        ///     Instance of the eraser
        /// </summary>
        public Eraser eraser;

        /// <summary>
        ///     Base color of the board
        /// </summary>
        public Color baseColor;

        [SerializeField] private float gpuRefreshRate = 0.1f;

        internal bool Modified;

        /// <summary>
        ///     Instance of this class
        /// </summary>
        public static Tools Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            InvokeRepeating(nameof(UpdateGPU), 0.5f, gpuRefreshRate);
        }

        /// <summary>
        ///     Updates the GPU rendering of the board's rendering every <see cref="gpuRefreshRate" />
        ///     The update is done only if the value of <see cref="Modified" /> has been changed
        /// </summary>
        private void UpdateGPU()
        {
            if (!Modified) return;

            Board.Instance.texture.Apply();
            Modified = false;
        }

        /// <summary>
        ///     Generates an array of colors
        /// </summary>
        /// <param name="color"> color used to populate the array </param>
        /// <param name="penSize"> size of the array </param>
        /// <returns> the array created </returns>
        public static Color[] GenerateSquare(Color color, float penSize)
        {
            return Enumerable.Repeat(color, (int) (penSize * penSize)).ToArray();
        }
    }
}