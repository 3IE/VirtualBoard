using UnityEngine;

namespace Board
{
    public class Board : MonoBehaviour
    {
        public Texture2D texture;
        public Vector2 textureSize = new(2048 * 0.6f, 2048 * 1.5f);
        public Tools tools;

        // Start is called before the first frame update
        private void Start()
        {
            Renderer r = GetComponent<Renderer>();
            
            texture = new Texture2D((int) textureSize.x, (int) textureSize.y);
            tools.baseColor = texture.GetPixel(0, 0);
            r.material.mainTexture = texture;
        }
    }
}
