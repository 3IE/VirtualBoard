using System;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    [Tooltip("The tip of the marker")]
    [SerializeField]
    private GameObject tip;

    private Renderer _renderer;
    private Color[] _colors;
    private float _tipHeight;

    private Transform tipTransform;
    private Board _board;

    private RaycastHit _touch;
    private Vector2 _touchPos;
    private Vector2 _lastTouchPos;
    private Quaternion _lastTouchRot;
    private bool _touchedLastFrame;

    private Queue<Modification> _modifications;

    // Start is called before the first frame update
    private void Start()
    {
        tipTransform = tip.transform;
        _renderer = tip.GetComponent<Renderer>();
        _tipHeight = tipTransform.localScale.y;

        _touchedLastFrame = false;
    }

    private void Update()
    {
        if (_modifications.TryDequeue(out Modification mod))
            ModifyTexture(mod);

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
        if (Physics.Raycast(tipTransform.position, transform.up, out _touch, _tipHeight) && _touch.transform.CompareTag("Board"))
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
                try
                {
                    ModifyTexture(x, y, _lastTouchPos.x, _lastTouchPos.y, _colors, _board.tools.penSize);
                }
                catch (ArgumentException e)
                {
                    #if UNITY_EDITOR
                    PrintVar.PrintDebug("Eraser: went out of board");
                    #endif

                    _board = null;
                    _touchedLastFrame = false;
                }
                finally
                {
                    // We lock the rotation of the marker while it is in contact with the board
                    transform.rotation = _lastTouchRot;

                    new Modification(x, y, _lastTouchPos.x, _lastTouchPos.y, _colors, _board.tools.penSize).Send(Event.EventCode.Marker);
                }
            }
            else
                // TODO generate shape depending on selected one
                _colors = Tools.generateSquare(_renderer.material.color, _board);

            _lastTouchPos = new Vector2(x, y);
            _lastTouchRot = transform.rotation;
            _touchedLastFrame = true;

            return;
        }

        _board = null;
        _touchedLastFrame = false;
    }

    private void ModifyTexture(int x, int y, float destX, float destY, Color[] colors, int penSize)
    {
        _board.texture.SetPixels(x, y, penSize, penSize, colors);

        // Interpolation
        for (float f = 0.01f; f < 1.00f; f += _board.tools.coverage)
        {
            int lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
            int lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);

            _board.texture.SetPixels(lerpX, lerpY, penSize, penSize, colors);
        }

        // We apply the changes
        _board.texture.Apply();
    }

    private void ModifyTexture(Modification modification)
    {
        ModifyTexture(modification.x, modification.y, modification.destX, modification.destY, modification.colors, modification.penSize);
    }

    public void AddModification(Modification modification)
    {
        _modifications.Enqueue(modification);
    }

    public void ResetColors(Board board)
    {
        _colors = Tools.generateSquare(_renderer.material.color, board);
    }

    // TODO add other shapes ?
}
