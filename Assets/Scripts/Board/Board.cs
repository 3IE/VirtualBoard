using UnityEngine;

namespace Board
{
    /// <summary>
    ///     Singleton class, holds the current state of the board
    /// </summary>
    public class Board : MonoBehaviour
    {
        /// <summary>
        ///     Texture of the board
        /// </summary>
        public Texture2D texture;

        /// <summary>
        ///     Size of the texture
        /// </summary>
        public Vector2 textureSize = new(2048, 2048);

        /// <summary>
        ///     Instance of the class
        /// </summary>
        public static Board Instance { get; private set; }

        private void Awake()
        {
            var r = GetComponent<Renderer>();

            texture                        = new Texture2D((int) textureSize.x, (int) textureSize.y);
            Tools.Tools.Instance.baseColor = texture.GetPixel(0, 0);
            r.material.mainTexture         = texture;

            Instance = this;
        }
    }
}