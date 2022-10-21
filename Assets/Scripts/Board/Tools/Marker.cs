using System;
using UnityEngine;
using Utils;

namespace Board.Tools
{
    /// <inheritdoc />
    public class Marker : WritingTool
    {
        [Tooltip("The tip of the marker")]
        [SerializeField]
        private GameObject tip;

        private Board   _board;
        private Color[] _colors;

        private Renderer _renderer;

        private Transform  _tipTransform;
        private RaycastHit _touch;
        private Vector2    _touchPos;

        /// <summary>
        ///     Position on the board touched last
        /// </summary>
        protected Vector2 LastTouchPos;

        // Start is called before the first frame update
        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateRotation();

            if (CanDraw)
                Tools.Instance.Modified = Draw() || Tools.Instance.Modified;
        }

        /// <summary>
        ///     Method called during start used to initialize the component
        /// </summary>
        protected virtual void Initialize()
        {
            _tipTransform = tip.transform;
            _renderer     = tip.GetComponent<Renderer>();

            TouchedLast = false;
        }

        /// <summary>
        ///     We check if we are in the boundaries of the board
        /// </summary>
        /// <param name="x"> The x coordinate of the point where the marker touches the board </param>
        /// <param name="y"> The y coordinate of the point where the marker touches the board </param>
        /// <returns>
        ///     <see langword="true" /> if we are in the boundaries of the board
        ///     <see langword="false" /> otherwise
        /// </returns>
        private bool InBound(int x, int y)
        {
            return x >= 0 && x <= _board.textureSize.x && y >= 0 && y <= _board.textureSize.y;
        }

        /// <summary>
        ///     Check if we are touching the board
        ///     if we are not, return <see langword="false" />
        ///     otherwise draw while touching (interpolation used to avoid drawing only dots) and return <see langword="true" />
        ///     we send the modifications at each frame
        /// </summary>
        private bool Draw()
        {
            // We check if we are touching the board with the marker
            if (Physics.Raycast(_tipTransform.position, _tipTransform.forward, out _touch,
                                _tipTransform.localScale.z / 2)
                && _touch.transform.CompareTag("Board"))
            {
                _board    ??= _touch.transform.GetComponent<Board>();
                _touchPos =   new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

                var x = (int) (_touchPos.x * _board.textureSize.x - penSize / 2);
                var y = (int) (_touchPos.y * _board.textureSize.y - penSize / 2);

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
                        ModifyTexture(x,              y,       LastTouchPos.x,
                                      LastTouchPos.y, _colors, penSize);
                    } catch (ArgumentException)
                    {
                        TouchedLast = false;
                    } finally
                    {
                        SendModification(x, y);

                        if (!TouchedLast)
                            _board = null;
                    }
                }
                else
                    _colors = GenerateShape();

                LastTouchPos = new Vector2(x, y);
                TouchedLast  = true;

                return true;
            }

            _board      = null;
            TouchedLast = false;

            return false;
        }

        /// <summary>
        ///     Generates a color array
        /// </summary>
        /// <returns> color array </returns>
        protected virtual Color[] GenerateShape()
        {
            return Tools.GenerateSquare(_renderer.material.color, penSize);

            // TODO generate shape depending on selected one
        }

        /// <summary>
        ///     Sends a modification to the other players
        /// </summary>
        /// <param name="x"> x coordinate of the starting point of the modification </param>
        /// <param name="y"> y coordinate of the starting point of the modification </param>
        protected virtual void SendModification(int x, int y)
        {
            new Modification(x, y, LastTouchPos.x,
                             LastTouchPos.y, _renderer.material.color,
                             penSize)
                .Send(EventCode.Marker);
        }

        /// <summary>
        ///     Applies a modification between two points on the texture of the board
        /// </summary>
        /// <param name="x"> x coordinate of the starting point </param>
        /// <param name="y"> y coordinate of the starting point </param>
        /// <param name="destX"> x coordinate of the ending point </param>
        /// <param name="destY"> y coordinate of the ending point </param>
        /// <param name="colors"> color array to apply at each step between the two points </param>
        /// <param name="size"> size of the array </param>
        private static void ModifyTexture(int   x,     int     y,      float destX,
                                          float destY, Color[] colors, float size)
        {
            var castSize = (int) size;

            Board.Instance.texture.SetPixels(x, y, castSize,
                                             castSize, colors);

            Board.Instance.texture.SetPixels((int) destX, (int) destY, castSize,
                                             castSize, colors);

            // Interpolation
            for (var f = 0.01f; f < 1.00f; f += Tools.Instance.coverage)
            {
                var lerpX = (int) Mathf.Lerp(destX, x, f);
                var lerpY = (int) Mathf.Lerp(destY, y, f);

                Board.Instance.texture.SetPixels(lerpX, lerpY, castSize,
                                                 castSize, colors);
            }
        }

        /// <summary>
        ///     Applies a modification
        /// </summary>
        /// <param name="modification"> Modification to apply </param>
        private static void ModifyTexture(Modification modification)
        {
            Color[] colors = Tools.GenerateSquare(modification.Color, modification.PenSize);

            ModifyTexture(modification.X,     modification.Y, modification.DestX,
                          modification.DestY, colors,
                          modification.PenSize);
        }

        /// <summary>
        ///     Modifies the board's texture and notifies the GPU to update its rendering
        /// </summary>
        /// <param name="modification"> modification to apply </param>
        public void AddModification(Modification modification)
        {
            ModifyTexture(modification);
            Tools.Instance.Modified = true;
        }

        // TODO add other shapes ?
    }
}