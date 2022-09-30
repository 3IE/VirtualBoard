using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using Event = Utils.Event;

namespace Board.Shapes
{
    public abstract class Shape : MonoBehaviour
    {
        public static ShapeSelector Selector;
        public static readonly Dictionary<int, Shape> Shapes = new();

        private static int _counter;
        private static int _defaultLayer;
        private static int _shapesLayer;

        private static readonly int Create1 = Shader.PropertyToID("_Creating");
        private static readonly int Modify1 = Shader.PropertyToID("_Modifying");
        private static readonly int Destroy1 = Shader.PropertyToID("_Destroying");

        public bool rotating;
        public bool moving;
        public bool resizing;
        
        protected byte ShapeId;

        protected float InitialDistance;
        protected Vector3 InitialScale;

        protected readonly List<IXRInteractor> Interactors = new(2);

        private int _id;

        private bool _locked;
        private bool _isOwner;
        
        private bool _deleting;

        private Material _mat;
        private Rigidbody _rigidbody;

        #region Unity

        private void Awake()
        {
            _mat = GetComponent<Renderer>().material;
            _rigidbody = GetComponent<Rigidbody>();
            _defaultLayer = LayerMask.NameToLayer("Static Shapes");
            _shapesLayer = LayerMask.NameToLayer("Shapes");
            _id = _counter++;
            
            Shapes.Add(_id, this);            
            
            Freeze();
        }

        private void Update()
        {
            if (moving)
                Move();
            else if (rotating)
                Rotate();
            else if (resizing)
                Resize();
        }

        #endregion

        #region Material

        private void Create()
        {
            _mat.SetFloat(Create1, 1);
        }

        private void Modify()
        {
            _mat.SetFloat(Modify1, 1);
        }

        public void Delete()
        {
            _mat.SetFloat(Destroy1, 1);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        private void StopAction()
        {
            _mat.SetFloat(Create1, 0);
            _mat.SetFloat(Modify1, 0);
            _mat.SetFloat(Destroy1, 0);
        }

        public void CreateAction(IXRInteractor interactor)
        {
            StopAction();
            Create();

            if (!_isOwner)
            {
                if (_locked)
                    return;

                _isOwner = true;
                _locked = true;

                SendOwnership();
            }

            Interactors.Add(interactor);

            UpdateAction();
        }

        public void StopCreateAction(IXRInteractor interactor)
        {
            StopAction();

            Interactors.Remove(interactor);

            UpdateActionDeselect();
            
            SendNewObject();
        }

        #endregion

        #region Selection

        public void OnHoverEnter(HoverEnterEventArgs args)
        {
            if (_deleting && ReferenceEquals(args.interactorObject, Selector.leftInteractor))
                Delete();
        }
        
        public void OnHoverExit(HoverExitEventArgs args)
        {
            if (_deleting && ReferenceEquals(args.interactorObject, Selector.leftInteractor))
                _mat.SetFloat(Destroy1, 0);
        }
        
        public void OnSelect(SelectEnterEventArgs args)
        {
            if(_deleting || Selector.currentShape is not null)
                return;
            
            if (!_isOwner)
            {
                if (_locked)
                    return;

                Selector.currentShape = this;
                
                _isOwner = true;
                _locked = true;

                SendOwnership();
            }

            Interactors.Add(args.interactorObject);
            
            Modify();

            UpdateAction();
        }

        public void OnDeselect(SelectExitEventArgs args)
        {
            if(_deleting)
                return;
            
            Selector.currentShape = null;
            
            Interactors.Clear();

            UpdateActionDeselect();
        }

        private void UpdateActionDeselect()
        {
            moving = false;
            resizing = false;
            _isOwner = false;
            _locked = false;

            gameObject.layer = _defaultLayer;
            
            StopAction();
            Freeze();
            SendOwnership();
        }

        private void UpdateAction()
        {
            moving = Interactors.Count == 1;
            resizing = Interactors.Count == 2;

            Unfreeze(); 
            
            if (moving)
            {
                InitialDistance = Vector3.Distance(transform.position, Interactors[0].transform.position);
                gameObject.layer = _shapesLayer;
            }

            if (!resizing) return;

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

        public static void DeletionMode(bool value)
        {
            foreach (var shape in Shapes.Values)
                shape._deleting = value;
        }

        #endregion

        #region Networking

        /// <summary>
        /// Sends a signal to create an object with these parameters
        /// </summary>
        protected void SendNewObject()
        {
            var transform1 = transform;
            var data = new object[] { transform1.position, transform1.rotation, ShapeId };

            SendData(Event.EventCode.SendNewObject, data);
        }

        /// <summary>
        /// Receives a signal to create an object with these parameters
        /// </summary>
        /// <param name="data"> the transform and id of the object </param>
        public static void ReceiveNewObject(object[] data)
        {
            var position = (Vector3)data[0];
            var rotation = (Quaternion)data[1];
            var shapeId = (byte)data[2];

            Instantiate(Selector.GetShape(shapeId), position, rotation);
        }

        /// <summary>
        /// Sends the new transform of the object to the other clients
        /// </summary>
        protected void SendTransform()
        {
            if (!_isOwner || !_locked) return;

            var transform1 = transform;
            object[] data = { transform1.position, transform1.rotation, transform1.localScale, _id };
            SendData(Event.EventCode.SendTransform, data);
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
        protected void SendDestroy()
        {
            if (!_isOwner || !_locked) return;

            SendData(Event.EventCode.SendDestroy, _id);
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
            SendData(Event.EventCode.SendOwnership, data);
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

        private static void SendData(Event.EventCode eventCode, object data = null)
        {
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte)eventCode, data, raiseEventOptions,
                SendOptions.SendReliable);
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
        
        public static int NumberOfShapes()
        {
            return Shapes.Count;
        }
    }
}