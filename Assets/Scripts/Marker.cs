using System.Linq;
using UnityEngine;

public class Marker : MonoBehaviour
{
    [Tooltip("The tip of the marker")]
    [SerializeField]
    private Transform tip;

    [Tooltip("How many pixels we want to change around the tip of the marker")]
    [SerializeField] 
    private int penSize = 5;

    [Tooltip("How much we want to interpolate the draw between two frames (lower values means more data is covered)")]
    [Range(0.01f, 1.00f)]
    [SerializeField]
    private float coverage;

    private Renderer _renderer;
    private Color[] _colors;
    private float _tipHeight;

    private Board _board;

    private RaycastHit _touch;
    private Vector2 _touchPos;
    private Vector2 _lastTouchPos;
    private Quaternion _lastTouchRot;
    private bool _touchedLastFrame;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _tipHeight = tip.localScale.y;

        _touchedLastFrame = false;
    }

    private void Update()
    {
        // TODO add tools
        Draw();
    }

    /// <summary>
    /// We check if we are touching the board with the marker
    /// </summary>
    /// <returns>
    /// True if we touch the board
    /// False otherwise
    /// </returns>
    private bool Colliding()
    {
        return Physics.Raycast(tip.position, transform.up, out _touch, _tipHeight) && _touch.transform.CompareTag("Board");
    }

    /// <summary>
    /// We check if we are in the boundaries of the board
    /// </summary>
    /// <param name="x"> The x coordinate of the point where the marker touches the board </param>
    /// <param name="y"> The y coordinate of the point where the marker touches the board </param>
    /// <returns>
    /// True if we are in the boundaries of the board
    /// False otherwise
    /// </returns>
    private bool InBound(int x, int y)
    {
        return x >= 0 && x <= _board.textureSize.x && y >= 0 && y <= _board.textureSize.y;
    }

    /// <summary>
    /// Check if we are touching the board
    /// if we are not, return
    /// otherwise draw while touching (interpolation used to avoid drawing only dots)
    /// when stop touching, send new texture to ar clients
    /// </summary>
    private void Draw()
    {
        if (Colliding())
        {
            if (_board == null)
                _board = _touch.transform.GetComponent<Board>();

            _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

            int x = (int)(_touchPos.x * _board.textureSize.x - (penSize / 2));
            int y = (int)(_touchPos.y * _board.textureSize.y - (penSize / 2));

            // If we are touching the board and in its boundaries, then we draw
            if (!InBound(x, y))
                return;

            if (_touchedLastFrame)
            {
                _board.texture.SetPixels(x, y, penSize, penSize, _colors);

                // Interpolation
                for (float f = 0.01f; f < 1.00f; f += coverage)
                {
                    int lerpX = (int) Mathf.Lerp(_lastTouchPos.x, x, f);
                    int lerpY = (int) Mathf.Lerp(_lastTouchPos.y, y, f);

                    _board.texture.SetPixels(lerpX, lerpY, penSize, penSize, _colors);
                }

                // We lock the rotation of the marker while it is in contact with the board
                transform.rotation = _lastTouchRot;

                // We apply the changes
                _board.texture.Apply();
            }
            else
            {
                // TODO generate shape depending on selected one
                generateCircle(x, y, _renderer.material.color);
            }

            _lastTouchPos = new Vector2(x, y);
            _lastTouchRot = transform.rotation;
            _touchedLastFrame = true;

            return;
        }

        _board = null;
        _touchedLastFrame = false;
    }

    /// <summary>
    /// Equivalent of the Draw method but with the base color of the board
    /// </summary>
    private void Erase()
    {
        // TODO
    }

    // TODO add other shapes ?

    private void generateSquare()
    {
        _colors = Enumerable.Repeat(_renderer.material.color, penSize * penSize).ToArray();
    }

    private void generateCircle(int x, int y, Color color)
    {
        _colors = _board.texture.GetPixels(x, y, penSize, penSize);
        
        Vector2Int center = new Vector2Int(penSize / 2, penSize / 2);

        for (int i = 0; i < penSize * penSize; i++)
        {
            Vector2Int pos = new Vector2Int(i % penSize, i / penSize);

            float distance = Vector2Int.Distance(pos, center);
            
            if (distance > penSize)
                _colors[i] = color;
        }
    }
}
