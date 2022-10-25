using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Utils;

namespace Board.Tools
{
    public sealed class KinematicMarker : MonoBehaviour
    {
        [SerializeField] private List<HoverInteractable> hover;

        [SerializeField] private float penSize;
        [SerializeField] private float snapDistance;

        [SerializeField] private Collider markerCollider;
        [SerializeField] private Collider tipCollider;

        [Tooltip("The tip of the marker")]
        [SerializeField]
        private GameObject tip;

        private Board   _board;
        private Color[] _colors;

        private Vector3 _direction;
        private bool    _grabbed;

        private XRGrabInteractable _grabInteractable;
        private Vector3            _initialPosition;
        private Quaternion         _initialRotation;
        private Vector3            _lastPosition;
        private Quaternion         _lastRot;

        private bool _previousCouldDraw;

        private Renderer _renderer;

        private Rigidbody _rigidbody;

        private bool _rotationLocked;

        private Vector3 _snapPosition;

        private Transform  _tipTransform;
        private RaycastHit _touch;
        private Vector2    _touchPos;

        private Transform _transform;

        /// <summary>
        ///     Boolean used to know if the tool is touching the board
        /// </summary>
        private bool _canDraw;

        /// <summary>
        ///     Controller holding the tool
        /// </summary>
        private XRBaseController _controller;

        private XRInteractorLineVisual _lineVisual;

        /// <summary>
        ///     Position on the board touched last
        /// </summary>
        private Vector2 _lastTouchPos;

        /// <summary>
        ///     Boolean used to know if the tool touched the board last frame
        /// </summary>
        private bool _touchedLast;

        // Start is called before the first frame update
        private void Start()
        {
            _transform        = transform;
            _tipTransform     = tip.transform;
            _direction        = -Board.Instance.transform.up;
            _grabInteractable = GetComponent<XRGrabInteractable>();
            _renderer         = tip.GetComponent<Renderer>();
            _rigidbody        = GetComponent<Rigidbody>();

            _touchedLast = false;
        }

        private void Update()
        {
            
            
            Tools.Instance.Modified = Draw() || Tools.Instance.Modified;
        }

        private void FixedUpdate()
        {
            var    hits   = new RaycastHit[3];
            Bounds bounds = tipCollider.bounds;

            int size = Physics.BoxCastNonAlloc(bounds.center, bounds.size / 2, _transform.forward,
                                               hits, _transform.rotation, 0f,
                                               LayerMask.GetMask("Ignore Raycast"));

            var couldDraw = false;

            for (var i = 0; !couldDraw && i < size; i++)
            {
                if (!hits[i].collider.CompareTag("Kinematic Board"))
                    continue;

                StartDrawing();
                couldDraw = true;
            }
            
            if (!_canDraw)
                return;

            Vector3    currentPos;
            Quaternion currentRot;

            if (_controller is not null)
            {
                Transform controllerTransform = _controller.transform;

                currentPos = controllerTransform.position;
                currentRot = controllerTransform.rotation;
            }
            else
            {
                currentPos = _transform.position;
                currentRot = _transform.rotation;
            }

            float distance = Vector3.Distance(_snapPosition, currentPos);

            if (distance >= snapDistance && currentPos.z + snapDistance <= _snapPosition.z)
            {
                StopDrawing();
                return;
            }

            _transform.position =
                new Vector3(currentPos.x, currentPos.y, _snapPosition.z);
            _transform.rotation = currentRot;

            if (_controller is null)
                return;

            _transform.Rotate(Vector3.right, 90f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Ceiling"))
                ResetPosition();
        }

        /// <summary>
        ///     highlights the item
        /// </summary>
        public void HoverTool()
        {
            if (_grabbed) return;

            foreach (HoverInteractable h in hover)
                h.Hover();
        }

        /// <summary>
        ///     Stops highlighting the item
        /// </summary>
        public void HoverExitTool()
        {
            if (_grabbed) return;

            foreach (HoverInteractable h in hover)
                h.HoverExit();
        }

        /// <summary>
        ///     Called when this tool is selected by a controller
        /// </summary>
        /// <param name="args"> properties tied to this event </param>
        public void OnSelected(SelectEnterEventArgs args)
        {
            IXRSelectInteractor interactor = args.interactorObject;
            _controller = interactor.transform.GetComponent<XRBaseController>();

            _lineVisual =   _controller.GetComponent<XRInteractorLineVisual>();
            _lineVisual ??= _controller.transform.parent.GetComponent<XRInteractorLineVisual>();

            if (_lineVisual is not null)
                _lineVisual.enabled = false;
        }

        /// <summary>
        ///     Called when the controller lets go of this tool
        /// </summary>
        public void OnDeselected()
        {
            if (_lineVisual is not null)
                _lineVisual.enabled = false;

            _controller = null;
        }

        private void StartDrawing()
        {
            _snapPosition                   = _transform.position;
            _grabInteractable.trackPosition = false;
            _grabInteractable.trackRotation = false;

            _canDraw               = true;
            tipCollider.enabled    = true;
            markerCollider.enabled = false;
        }

        private void StopDrawing()
        {
            _grabInteractable.trackPosition = true;
            _grabInteractable.trackRotation = true;

            _board                 = null;
            _canDraw               = false;
            _touchedLast           = false;
            tipCollider.enabled    = false;
            markerCollider.enabled = true;
        }

        private void ResetPosition()
        {
            _rigidbody.velocity        = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            _transform.position = _initialPosition;
            _transform.rotation = _initialRotation;
        }

        #region Draw

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
            if (Physics.Raycast(_tipTransform.position, _direction, out _touch,
                                _tipTransform.localScale.z / 2 + 0.01f, LayerMask.GetMask("Default"))
                && _touch.transform.CompareTag("Board"))
            {
                _board    ??= _touch.transform.GetComponent<Board>();
                _touchPos =   new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

                var x = (int) (_touchPos.x * _board.textureSize.x - penSize / 2);
                var y = (int) (_touchPos.y * _board.textureSize.y - penSize / 2);

                // If we are touching the board and in its boundaries, then we draw
                if (!InBound(x, y))
                    return false;

                if (_touchedLast)
                {
                    if (Vector2.Distance(new Vector2(x, y), _lastTouchPos) < 0.01f)
                        return false;

                    // ReSharper disable once Unity.NoNullPropagation
                    _controller?.SendHapticImpulse(0.1f, 0.1f);

                    try
                    {
                        ModifyTexture(x,               y,       _lastTouchPos.x,
                                      _lastTouchPos.y, _colors, penSize);
                    } catch (ArgumentException)
                    {
                        _touchedLast = false;
                    } finally
                    {
                        SendModification(x, y);

                        if (!_touchedLast)
                            _board = null;
                    }
                }
                else
                    _colors = GenerateShape();

                _lastTouchPos = new Vector2(x, y);
                _touchedLast  = true;

                return true;
            }

            _board       = null;
            _touchedLast = false;

            return false;
        }

        /// <summary>
        ///     Generates a color array
        /// </summary>
        /// <returns> color array </returns>
        private Color[] GenerateShape()
        {
            return Tools.GenerateSquare(_renderer.material.color, penSize);

            // TODO generate shape depending on selected one
        }

        /// <summary>
        ///     Sends a modification to the other players
        /// </summary>
        /// <param name="x"> x coordinate of the starting point of the modification </param>
        /// <param name="y"> y coordinate of the starting point of the modification </param>
        private void SendModification(int x, int y)
        {
            new Modification(x, y, _lastTouchPos.x,
                             _lastTouchPos.y, _renderer.material.color,
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
        public static void AddModification(Modification modification)
        {
            ModifyTexture(modification);
            Tools.Instance.Modified = true;
        }

        #endregion
    }
}