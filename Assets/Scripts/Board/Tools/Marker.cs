using System;
using System.Collections.Generic;
using UnityEngine;
using Event = Utils.Event;

namespace Board.Tools
{
    public class Marker : WritingTool
    {
        [Tooltip("The tip of the marker")] [SerializeField]
        private GameObject tip;

        private Renderer _renderer;
        private Color[] _colors;

        private Transform _tipTransform;
        private RaycastHit _touch;
        private Vector2 _touchPos;

        private Queue<Modification> _modifications;

        protected Vector2 LastTouchPos;
        protected Board Board;

        protected virtual void Initialize()
        {
            _tipTransform = tip.transform;
            _renderer = tip.GetComponent<Renderer>();

            TouchedLast = false;
            _modifications = new Queue<Modification>(256);
        }
        
        // Start is called before the first frame update
        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateRotation();

            if (_modifications.TryDequeue(out var mod))
                ModifyTexture(mod);

            if (canDraw)
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
            return x >= 0 && x <= Board.textureSize.x && y >= 0 && y <= Board.textureSize.y;
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
            if (Physics.Raycast(_tipTransform.position, transform.up, out _touch, .02f) &&
                _touch.transform.CompareTag("Board"))
            {
                Board ??= _touch.transform.GetComponent<Board>();
                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

                var x = (int)(_touchPos.x * Board.textureSize.x - Board!.tools.penSize / 2);
                var y = (int)(_touchPos.y * Board.textureSize.y - Board!.tools.penSize / 2);

                // If we are touching the board and in its boundaries, then we draw
                if (!InBound(x, y))
                    return;

                if (TouchedLast)
                {
                    if (Vector2.Distance(new Vector2(x, y), LastTouchPos) < 0.01f)
                        return;

                    try
                    {
                        ModifyTexture(x, y, LastTouchPos.x, LastTouchPos.y, _colors, Board.tools.penSize);
                    }
                    catch (ArgumentException)
                    {
                        TouchedLast = false;
                    }
                    finally
                    {
                        SendModification(x, y);

                        if (!TouchedLast)
                            Board = null;
                    }
                }
                else
                    _colors = GenerateShape();

                LastTouchPos = new Vector2(x, y);
                TouchedLast = true;

                return;
            }

            Board = null;
            TouchedLast = false;
        }

        protected virtual Color[] GenerateShape()
        {
            return Tools.GenerateSquare(_renderer.material.color, Board);
            // TODO generate shape depending on selected one
        }

        protected virtual void SendModification(int x, int y)
        {
            new Modification(x, y, LastTouchPos.x, LastTouchPos.y, _renderer.material.color,
                    Board!.tools.penSize)
                .Send(Event.EventCode.Marker);
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

        // TODO add other shapes ?
    }
}