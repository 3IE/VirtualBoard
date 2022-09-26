using System;
using System.Collections.Generic;
using UnityEngine;
using Event = Utils.Event;

namespace Board
{
    public class Marker : WritingTool
    {
        [Tooltip("The tip of the marker")] [SerializeField]
        private GameObject tip;

        private Renderer _renderer;
        private Color[] _colors;

        private Transform _tipTransform;
        private Board _board;

        private RaycastHit _touch;
        private Vector2 _touchPos;
        private Vector2 _lastTouchPos;
        private bool _touchedLastFrame;

        private Queue<Modification> _modifications;

        // Start is called before the first frame update
        private void Start()
        {
            _tipTransform = tip.transform;
            _renderer = tip.GetComponent<Renderer>();

            _touchedLastFrame = false;
            _modifications = new Queue<Modification>();
        }

        private void Update()
        {
            UpdateRotation();

            if (_modifications.TryDequeue(out var mod))
                ModifyTexture(mod);

            if (CanDraw)
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
            if (Physics.Raycast(_tipTransform.position, transform.up, out _touch, .5f) &&
                _touch.transform.CompareTag("Board"))
            {
                _board ??= _touch.transform.GetComponent<Board>();
                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

                var x = (int)(_touchPos.x * _board.textureSize.x - _board!.tools.penSize / 2);
                var y = (int)(_touchPos.y * _board.textureSize.y - _board!.tools.penSize / 2);

                // If we are touching the board and in its boundaries, then we draw
                if (!InBound(x, y))
                    return;

                if (_touchedLastFrame)
                {
                    try
                    {
                        ModifyTexture(x, y, _lastTouchPos.x, _lastTouchPos.y, _colors, _board.tools.penSize);
                    }
                    catch (ArgumentException)
                    {
                        _board = null;
                        _touchedLastFrame = false;
                    }
                    finally
                    {
                        new Modification(x, y, _lastTouchPos.x, _lastTouchPos.y, _renderer.material.color,
                                _board!.tools.penSize)
                            .Send(Event.EventCode.Marker);
                    }
                }
                else
                    // TODO generate shape depending on selected one
                    _colors = Tools.GenerateSquare(_renderer.material.color, _board);

                _lastTouchPos = new Vector2(x, y);
                _touchedLastFrame = true;

                return;
            }

            _board = null;
            _touchedLastFrame = false;
        }

        private void ModifyTexture(int x, int y, float destX, float destY, Color[] colors, int penSize)
        {
            boardObject.texture.SetPixels(x, y, penSize, penSize, colors);

            // Interpolation
            for (var f = 0.01f; f < 1.00f; f += boardObject.tools.coverage)
            {
                var lerpX = (int)Mathf.Lerp(destX, x, f);
                var lerpY = (int)Mathf.Lerp(destY, y, f);

                boardObject.texture.SetPixels(lerpX, lerpY, penSize, penSize, colors);
            }

            // We apply the changes
            boardObject.texture.Apply();
        }

        private void ModifyTexture(Modification modification)
        {
            var colors = Tools.GenerateSquare(modification.Color, boardObject);
            ModifyTexture(modification.X, modification.Y, modification.DestX, modification.DestY, colors,
                modification.PenSize);
        }

        public void AddModification(Modification modification)
        {
            _modifications.Enqueue(modification);
        }

        public void ResetColors(Board board)
        {
            _colors = Tools.GenerateSquare(_renderer.material.color, board);
        }

        // TODO add other shapes ?
    }
}