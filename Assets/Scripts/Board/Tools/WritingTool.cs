using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Utils;

namespace Board.Tools
{
    /// <summary>
    ///     Base class for the tools writing on the board
    /// </summary>
    public class WritingTool : MonoBehaviour
    {
        [SerializeField] private List<HoverInteractable> hover;

        /// <summary>
        ///     Size used to know the number of pixels we should modify
        /// </summary>
        [SerializeField]
        protected float penSize;

        private Vector3    _initialPosition;
        private Quaternion _initialRotation;
        private Vector3    _lastPosition;

        private Quaternion _lastRot;
        private Rigidbody  _rigidbody;

        private bool _rotationLocked;

        private Transform _transform;

        /// <summary>
        ///     Boolean used to know if the tool is touching the board
        /// </summary>
        protected bool CanDraw;

        /// <summary>
        ///     Controller holding the tool
        /// </summary>
        protected XRBaseController Controller;

        /// <summary>
        ///     Boolean used to know if the tool touched the board last frame
        /// </summary>
        protected bool TouchedLast;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _transform = transform;

            TouchedLast = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.collider.CompareTag("Board")) return;

            TouchedLast = false;

            _rotationLocked = true;
            CanDraw         = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!collision.collider.CompareTag("Board")) return;

            TouchedLast = false;

            _rotationLocked = false;
            CanDraw         = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Ceiling")) return;

            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            _transform.position = _initialPosition;
            _transform.rotation = _initialRotation;
        }

        /// <summary>
        ///     Locks the rotation of the tool if it's touching the board
        /// </summary>
        protected void UpdateRotation()
        {
            if (_rotationLocked)
            {
                if (TouchedLast)
                {
                    transform.rotation = _lastRot;

                    if (transform.position.z <= _lastPosition.z) return;

                    Vector3 position = _transform.position;

                    _lastPosition.x = position.x;
                    _lastPosition.y = position.y;

                    _transform.position = _lastPosition;
                }
                else
                {
                    _lastRot      = _transform.rotation;
                    _lastPosition = _transform.position;
                }
            }
            else
                TouchedLast = false;
        }

        /// <summary>
        ///     highlights the item
        /// </summary>
        public void HoverTool()
        {
            foreach (HoverInteractable h in hover)
                h.Hover();
        }

        /// <summary>
        ///     Stops highlighting the item
        /// </summary>
        public void HoverExitTool()
        {
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
            Controller = interactor.transform.GetComponent<XRBaseController>();
        }

        /// <summary>
        ///     Called when the controller lets go of this tool
        /// </summary>
        public void OnDeselected()
        {
            Controller = null;
        }
    }
}