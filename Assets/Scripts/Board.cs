using UnityEngine;

public class Board : MonoBehaviour
{
    public Texture2D texture;
    public Vector2 textureSize = new Vector2(2048 * 1.5f, 2048 * 0.6f);
    public Tools tools;

    // Start is called before the first frame update
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();

        texture = new Texture2D((int) textureSize.x, (int) textureSize.y);
        tools.baseColor = texture.GetPixel(0, 0);
        renderer.material.mainTexture = texture;
    }
}
