using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Utils;
using EventCode = Utils.EventCode;

namespace Board.Shapes
{
    /// <summary>
    /// 3D Object which can be spawned in the scene
    /// </summary>
    public abstract class Shape : MonoBehaviour
    {
        private static int _counter;
        private static int _defaultLayer;
        private static int _shapesLayer;

        private static readonly int Create1 = Shader.PropertyToID("_Creating");
        private static readonly int Modify1 = Shader.PropertyToID("_Modifying");
        private static readonly int Destroy1 = Shader.PropertyToID("_Destroying");

        private int _id;

        private bool _locked;
        private bool _isOwner;
        private bool _created;
        private bool _deleting;

        private Rigidbody _rigidbody;

        /// <summary>
        /// List of interactors currently holding this shape
        /// </summary>
        protected readonly List<IXRInteractor> Interactors = new(2);
        /// <summary>
        /// Id corresponding to the type of this shape
        /// </summary>
        protected internal byte ShapeId;

        /// <summary>
        /// Material of the object
        /// </summary>
        protected Material Mat;

        internal Vector3 InitialScale;
        internal float InitialDistance;

        internal bool Rotating;
        internal bool Moving;
        internal bool Resizing;

        /// <summary>
        /// Holds a list of the existing shapes with their <see cref="_id"/> as a key
        /// </summary>
        public static readonly Dictionary<int, Shape> Shapes = new();

        #region Unity

#if DEBUG
        private void OnDestroy()
        {
            DebugPanel.Instance.RemoveObject();
        }
#endif

        private void Awake()
        {
            Mat = GetComponent<Renderer>().material;
            _rigidbody = GetComponent<Rigidbody>();
            _defaultLayer = LayerMask.NameToLayer("Static Shapes");
            _shapesLayer = LayerMask.NameToLayer("Shapes");
            _id = _counter++;

            Shapes.Add(_id, this);

            Freeze();

            _created = true;
            
            #if DEBUG
            DebugPanel.Instance.AddObject();
            #endif
        }

        private void Update()
        {
            if (Moving)
                Move();
            else if (Rotating)
                Rotate();
            else if (Resizing)
                Resize();
        }

        #endregion

        #region Material

        private void Create()
        {
            Mat.SetFloat(Create1, 1);
        }

        private void Modify()
        {
            Mat.SetFloat(Modify1, 1);
        }

        internal void Delete()
        {
            Mat.SetFloat(Destroy1, 1);
        }

        internal void CallDestroy(bool creation)
        {
            Shapes.Remove(_id);

            if (!creation)
                SendDestroy();

            Destroy(gameObject);
        }

        private void StopAction()
        {
            Mat.SetFloat(Create1, 0);
            Mat.SetFloat(Modify1, 0);
            Mat.SetFloat(Destroy1, 0);
        }

        internal void CreateAction(IXRInteractor interactor)
        {
            _created = false;
            
            StopAction();
            Create();

            if (!_isOwner)
            {
                if (_locked)
                    return;

                _isOwner = true;
                _locked = true;
            }

            Interactors.Add(interactor);

            UpdateAction();
        }

        internal void StopCreateAction(IXRInteractor interactor)
        {
            StopAction();

            if (transform.position.y < -1.5f)
            {
                CallDestroy(true);
                return;
            }

            Interactors.Remove(interactor);

            UpdateActionDeselect();

            SendNewObject();

            _created = true;
        }

        #endregion

        #region Selection

        /// <summary>
        /// Is called when an interactor hovers over this shape
        /// </summary>
        /// <param name="args"> Properties tied to the event </param>
        public void OnHoverEnter(HoverEnterEventArgs args)
        {
            if (_deleting && ReferenceEquals(args.interactorObject, ShapeSelector.Instance.leftInteractor))
                Delete();
        }

        /// <summary>
        /// Is called when an interactor stops hovering over this shape
        /// </summary>
        /// <param name="args"> Properties tied to the event </param>
        public void OnHoverExit(HoverExitEventArgs args)
        {
            if (_deleting && ReferenceEquals(args.interactorObject, ShapeSelector.Instance.leftInteractor))
                Mat.SetFloat(Destroy1, 0);
        }

        /// <summary>
        /// Is called when an interactor selects this shape
        /// </summary>
        /// <param name="args"> Properties tied to the event </param>
        public void OnSelect(SelectEnterEventArgs args)
        {
            if (_deleting)
                return;

            if (ShapeSelector.Instance.currentShape is not null && !ReferenceEquals(ShapeSelector.Instance.currentShape, this))
                return;

            if (!_isOwner)
            {
                if (_locked)
                    return;

                ShapeSelector.Instance.currentShape = this;

                _isOwner = true;
                _locked = true;

                SendOwnership();
            }

            Interactors.Add(args.interactorObject);

            Modify();

            UpdateAction();
        }

        /// <summary>
        /// Is called when an interactor lets go of this shape
        /// </summary>
        /// <param name="args"> Properties tied to the event </param>
        public void OnDeselect(SelectExitEventArgs args)
        {
            if (_deleting)
                return;

            ShapeSelector.Instance.currentShape = null;

            Interactors.Clear();

            UpdateActionDeselect();
        }

        private void UpdateActionDeselect()
        {
            Moving = false;
            Rotating = false;
            Resizing = false;
            _isOwner = false;
            _locked = false;

            gameObject.layer = _defaultLayer;

            ShapeSelector.Instance.StopChangeDistance();

            StopAction();
            Freeze();
            SendOwnership();
        }

        private void UpdateAction()
        {
            Moving = Interactors.Count == 1;
            Resizing = Interactors.Count == 2;

            Unfreeze();

            if (Moving)
            {
                InitialDistance = Vector3.Distance(transform.position, Interactors[0].transform.position);
                gameObject.layer = _shapesLayer;

                ShapeSelector.Instance.StartChangeDistance();
            }

            if (!Resizing) return;

            ShapeSelector.Instance.StopChangeDistance();

            InitialDistance = Vector3.Distance(Interactors[0].transform.position, Interactors[1].transform.position);
            if (InitialDistance == 0)
                InitialDistance = 1;

            InitialScale = transform.localScale;
        }

        private void Freeze()
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        private void Unfreeze()
        {
            _rigidbody.constraints = RigidbodyConstraints.None;
        }

        internal static void DeletionMode(bool value)
        {
            foreach (var shape in Shapes.Values)
                shape._deleting = value;
        }

        #endregion

        #region Networking

        /// <summary>
        /// Sends a signal to create an object with these parameters
        /// </summary>
        private void SendNewObject()
        {
            Debug.Log("Sending created object");

            var transform1 = transform;
            var data = new object[] { transform1.position, transform1.rotation, ShapeId };

            SendData(EventCode.SendNewObject, data);
        }

        /// <summary>
        /// Receives a signal to create an object with these parameters
        /// </summary>
        /// <param name="data"> the transform and id of the object </param>
        public static void ReceiveNewObject(object[] data)
        {
            Debug.Log("Receiving created object");

            var position = (Vector3)data[0];
            var rotation = (Quaternion)data[1];
            var shapeId = (byte)data[2];

            Instantiate(ShapeSelector.Instance.GetShape(shapeId), position, rotation);
        }

        /// <summary>
        /// Sends the new transform of the object to the other clients
        /// </summary>
        protected void SendTransform()
        {
            if (!_isOwner || !_locked || !_created) return;

            var transform1 = transform;
            object[] data = { transform1.position, transform1.rotation, transform1.localScale, _id };
            SendData(EventCode.SendTransform, data);
        }

        /// <summary>
        /// Gets the new transform of the object from the server and applies it to the object.
        /// </summary>
        public static void ReceiveTransform(object[] data)
        {
            var position = (Vector3)data[0];
            var rotation = (Quaternion)data[1];
            var scale = (Vector3)data[2];
            var id = (int)data[3];

            var shape = Shapes[id].transform;

            shape.position = position;
            shape.rotation = rotation;
            shape.localScale = scale;
        }

        /// <summary>
        /// Tells the other clients to destroy this object
        /// </summary>
        private void SendDestroy()
        {
            Debug.Log("Sending destroy");

            SendData(EventCode.SendDestroy, _id);
        }

        /// <summary>
        /// Destroy the object with the corresponding id
        /// </summary>
        public static void ReceiveDestroy(int id)
        {
            Shapes[id].Delete();
            Shapes.Remove(id);
        }

        /// <summary>
        /// Warns the other clients that this object can't be modified
        /// </summary>
        private void SendOwnership()
        {
            if (!_isOwner || !_locked) return;

            object[] data = { _isOwner, _id };
            SendData(EventCode.SendOwnership, data);
        }

        /// <summary>
        /// Update this object so that it can't be modified until told otherwise
        /// </summary>
        public static void ReceiveOwnership(object[] data)
        {
            var owned = (bool)data[0];
            var id = (int)data[1];

            Shapes[id]._locked = owned;
        }

        private static void SendData(EventCode eventCode, object data = null)
        {
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte)eventCode, data, raiseEventOptions,
                SendOptions.SendReliable);

#if DEBUG
            DebugPanel.Instance.AddObjectSent();
#endif
        }

        #endregion

        #region Abstract

        /// <summary>
        /// Moves the object along the ray of the controller selecting it
        /// </summary>
        protected abstract void Move();

        /// <summary>
        /// Resizes the object according to the distance between the two controllers
        /// </summary>
        protected abstract void Resize();

        /// <summary>
        /// Rotates the object according to the angle of the current controller
        /// </summary>
        protected abstract void Rotate();

        #endregion

        /// <summary>
        /// Returns the number of shapes currently existing
        /// </summary>
        /// <returns> number of shapes </returns>
        public static int NumberOfShapes()
        {
            return Shapes.Count;
        }
    }
}