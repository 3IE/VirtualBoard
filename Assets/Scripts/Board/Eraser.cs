using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Event = Utils.Event;

namespace Board
{
    public class Eraser : WritingTool
    {
        private Color[] _colors;

        private Board _board;

        private RaycastHit _touch;
        private Vector2 _touchPos;
        private Vector2 _lastTouchPos;
        private bool _touchedLastFrame;

        private Queue<Modification> _modifications;

        // Start is called before the first frame update
        void Start()
        {
            _touchedLastFrame = false;
            _modifications = new Queue<Modification>();
        }

        private void Update()
        {
            UpdateRotation();

            if (_modifications.TryDequeue(out Modification mod))
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
            if (Physics.Raycast(transform.position, transform.up, out _touch, 0.5f) && _touch.transform.CompareTag("Board"))
            {
                if (_board == null)
                    _board = _touch.transform.GetComponent<Board>();

                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

                int x = (int)(_touchPos.x * _board.textureSize.x - _board!.tools.penSize / 2);
                int y = (int)(_touchPos.y * _board.textureSize.y - _board!.tools.penSize / 2);

                // If we are touching the board and in its boundaries, then we draw
                if (!InBound(x, y))
                    return;

                if (_touchedLastFrame)
                {
                    try {
                        ModifyTexture(x, y, _lastTouchPos.x, _lastTouchPos.y, _colors, _board.tools.penSize);
                    }
                    catch (ArgumentException)
                    {
#if UNITY_EDITOR
                        PrintVar.PrintDebug("Eraser: went out of board");
#endif

                        _board = null;
                        _touchedLastFrame = false;
                    }
                    finally
                    {
                        new Modification(x, y, _lastTouchPos.x, _lastTouchPos.y, _colors, _board!.tools.penSize)
                            .Send(Event.EventCode.Eraser);
                    }
                }
                else
                    // TODO generate shape depending on selected one
                    _colors = Tools.GenerateSquare(_board.tools.baseColor, _board);

                _lastTouchPos = new Vector2(x, y);
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
            _colors = Tools.GenerateSquare(board.tools.baseColor, board);
        }

        // TODO add other shapes ?
    }
}
