using UnityEngine;

public class BoardSetter : MonoBehaviour
{
    [SerializeField] private Renderer      boardRenderer;
    [SerializeField] private Renderer      boardResetRenderer;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private GameObject    boardSetter;

    private Texture2D _texture;
    private Material  _boardMaterial;
    private Material  _boardResetMaterial;

    private static readonly int Color = Shader.PropertyToID("_Color");
    private static          int _baseLayer;
    private static          int _resetLayer;

    // Start is called before the first frame update
    private void Start()
    {
        _boardMaterial      = boardRenderer.material;
        _boardResetMaterial = boardResetRenderer.material;

        _baseLayer  = LayerMask.NameToLayer("test");
        _resetLayer = LayerMask.NameToLayer("Tool Tip");

        ChangeBaseColor();

        _texture = new Texture2D(renderTexture.width, renderTexture.height);
        boardSetter.GetComponent<Renderer>().material.mainTexture = _texture;
    }

    private void ChangeBaseColor()
    {
        _boardResetMaterial.color = _boardMaterial.GetColor(Color);
    }

    public void ResetBoard()
    {
        gameObject.layer = _resetLayer;
    }

    public void StopResetBoard()
    {
        gameObject.layer = _baseLayer;
    }

    private void SetTexture(Texture2D texture)
    {
        _texture.SetPixels(texture.GetPixels());
        _texture.Apply();

        boardSetter.SetActive(true);

        Invoke(nameof(DeactivateBoardSetter), 0.01f);
    }

    private void DeactivateBoardSetter()
    {
        boardSetter.SetActive(false);
    }
}