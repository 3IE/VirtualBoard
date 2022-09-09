using System;
using UnityEngine;

public class Eraser : MonoBehaviour
{
    private Color[] _colors;

    private Board _board;

    private RaycastHit _touch;
    private Vector2 _touchPos;
    private Vector2 _lastTouchPos;
    private Quaternion _lastTouchRot;
    private bool _touchedLastFrame;

    // Start is called before the first frame update
    void Start()
    {
        _touchedLastFrame = false;
    }

    private void Update()
    {
        // TODO add tools
        Draw();
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
        // We check if we are touching the board with the marker
        if (Physics.Raycast(transform.position, transform.up, out _touch, 0.1f) && _touch.transform.CompareTag("Board"))
        {
            if (_board == null)
                _board = _touch.transform.GetComponent<Board>();

            _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

            int x = (int)(_touchPos.x * _board.textureSize.x - (_board.tools.penSize / 2));
            int y = (int)(_touchPos.y * _board.textureSize.y - (_board.tools.penSize / 2));

            // If we are touching the board and in its boundaries, then we draw
            if (!InBound(x, y))
                return;

            if (_touchedLastFrame)
            {
                try {
                    _board.texture.SetPixels(x, y, _board.tools.penSize, _board.tools.penSize, _colors);

                    // Interpolation
                    for (float f = 0.01f; f < 1.00f; f += _board.tools.coverage)
                    {
                        int lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
                        int lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);

                        _board.texture.SetPixels(lerpX, lerpY, _board.tools.penSize, _board.tools.penSize, _colors);
                    }
                }
                catch (ArgumentException e)
                {
                    Debug.LogError(e.Message);

                    _board = null;
                    _touchedLastFrame = false;
                }
                finally
                {
                    // We lock the rotation of the marker while it is in contact with the board
                    transform.rotation = _lastTouchRot;

                    // We apply the changes
                    _board.texture.Apply();
                }
            }
            else
                // TODO generate shape depending on selected one
                _colors = Tools.generateSquare(_board.tools.baseColor, _board);

            _lastTouchPos = new Vector2(x, y);
            _lastTouchRot = transform.rotation;
            _touchedLastFrame = true;

            return;
        }

        // TODO Uncomment to allow sending to the other clients
        // if (_touchedLastFrame)
        //     Tools.SendNewTextureEvent(_board);

        _board = null;
        _touchedLastFrame = false;
    }

    public void resetColors(Board board)
    {
        _colors = Tools.generateSquare(board.tools.baseColor, board);
    }

    // TODO add other shapes ?
}
