using UnityEngine;

public class Board : MonoBehaviour
{
    public Texture2D texture;
    public Vector2 textureSize = new Vector2(2048, 2048);

    // Start is called before the first frame update
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();

        texture = new Texture2D((int) textureSize.x, (int) textureSize.y);
        renderer.material.mainTexture = texture;
    }
}
