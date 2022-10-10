using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
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

        //private Queue<Modification> _modifications;

        protected Vector2 LastTouchPos;
        private Board _board;

        private bool _modified = false;
        [SerializeField] private float gpuRefreshRate = 0.1f;

        protected virtual void Initialize()
        {
            _tipTransform = tip.transform;
            _renderer = tip.GetComponent<Renderer>();

            TouchedLast = false;
            //_modifications = new Queue<Modification>(256);
        }

        // Start is called before the first frame update
        private void Start()
        {
            Initialize();
            
            InvokeRepeating(nameof(UpdateGPU), 0.5f, gpuRefreshRate);
        }

        private void UpdateGPU()
        {
            if (!_modified) return;
            
            boardObject.texture.Apply();
            _modified = false;
        }
        
        private void Update()
        {
            UpdateRotation();
            
            //if (_modifications.TryDequeue(out var mod))
            //{
            //    ModifyTexture(mod);
            //    _modified = true;

#if DEBUG
                //DebugPanel.Instance.RemoveBoardQueue();
#endif
            //}

            if (canDraw)
                _modified = Draw() || _modified;
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
        private bool Draw()
        {
            // We check if we are touching the board with the marker
            if (Physics.Raycast(_tipTransform.position, _tipTransform.forward, out _touch,
                    _tipTransform.localScale.z / 2) &&
                _touch.transform.CompareTag("Board"))
            {
                _board ??= _touch.transform.GetComponent<Board>();
                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

                var x = (int)(_touchPos.x * _board.textureSize.x - penSize / 2);
                var y = (int)(_touchPos.y * _board.textureSize.y - penSize / 2);

                // If we are touching the board and in its boundaries, then we draw
                if (!InBound(x, y))
                    return false;

                if (TouchedLast)
                {
                    if (Vector2.Distance(new Vector2(x, y), LastTouchPos) < 0.01f)
                        return false;

                    // ReSharper disable once Unity.NoNullPropagation
                    Controller?.SendHapticImpulse(0.1f, 0.1f);

                    try
                    {
                        ModifyTexture(x, y, LastTouchPos.x, LastTouchPos.y, _colors, penSize);
                    }
                    catch (ArgumentException)
                    {
                        TouchedLast = false;
                    }
                    finally
                    {
                        SendModification(x, y);

                        if (!TouchedLast)
                            _board = null;
                    }
                }
                else
                    _colors = GenerateShape();

                LastTouchPos = new Vector2(x, y);
                TouchedLast = true;

                return true;
            }

            _board = null;
            TouchedLast = false;

            return false;
        }

        protected virtual Color[] GenerateShape()
        {
            return Tools.GenerateSquare(_renderer.material.color, penSize);
            // TODO generate shape depending on selected one
        }

        protected virtual void SendModification(int x, int y)
        {
            new Modification(x, y, LastTouchPos.x, LastTouchPos.y, _renderer.material.color,
                    penSize)
                .Send(Event.EventCode.Marker);
        }

        private void ModifyTexture(int x, int y, float destX, float destY, Color[] colors, float size)
        {
            var castSize = (int)size;

            boardObject.texture.SetPixels(x, y, castSize, castSize, colors);
            boardObject.texture.SetPixels((int)destX, (int)destY, castSize, castSize, colors);

            // Interpolation
            for (var f = 0.01f; f < 1.00f; f += boardObject.tools.coverage)
            {
                var lerpX = (int)Mathf.Lerp(destX, x, f);
                var lerpY = (int)Mathf.Lerp(destY, y, f);
            
                boardObject.texture.SetPixels(lerpX, lerpY, castSize, castSize, colors);
            }
        }

        private void ModifyTexture(Modification modification)
        {
            var colors = Tools.GenerateSquare(modification.Color, penSize);

            ModifyTexture(modification.X, modification.Y, modification.DestX, modification.DestY, colors,
                modification.PenSize);
        }

        public void AddModification(Modification modification)
        {
            //_modifications.Enqueue(modification);
            ModifyTexture(modification);            
            _modified = true;


#if DEBUG
            //DebugPanel.Instance.AddBoardQueue();
#endif
        }

        // TODO add other shapes ?
    }
}