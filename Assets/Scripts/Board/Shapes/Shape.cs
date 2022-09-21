using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Event = Utils.Event;

namespace Board.Shapes
{
    public abstract class Shape : MonoBehaviour
    {
        public static ShapeSelector Selector;

        protected byte ShapeId;

        protected float InitialDistance;
        protected Vector3 InitialScale;

        protected List<IXRInteractor> Interactors;

        private int _id;

        private bool _locked;
        private bool _isOwner;

        private bool _moving;
        private bool _resizing;

        private Material _mat;

        private static readonly int Create1 = Shader.PropertyToID("_Create");
        private static readonly int Modify1 = Shader.PropertyToID("_Modify");
        private static readonly int Destroy1 = Shader.PropertyToID("_Destroy");

        private static int _defaultLayer;
        private static int _shapesLayer;

        private static readonly List<Shape> Shapes = new();

        #region Unity

        private void Start()
        {
            _mat = GetComponent<Renderer>().material;
            Interactors = new List<IXRInteractor>(2);
            GetComponent<Collider>().GetInstanceID();

            _defaultLayer = LayerMask.NameToLayer("Default");
            _shapesLayer = LayerMask.NameToLayer("Shapes");

            Create();

#if UNITY_EDITOR
            Interactors.Add(GameObject.Find("RightHand Controller").GetComponent<XRRayInteractor>());
            InitialDistance = 15;
            gameObject.layer = _shapesLayer;
            _moving = true;
#endif

            _id = Shapes.Count;
            Shapes.Add(this);
        }

        private void Update()
        {
            if (_moving)
                Move();
            if (_resizing)
                Resize();
        }

        #endregion

        #region Material

        public void Create()
        {
            _mat.SetFloat(Create1, 1);
        }

        public void Modify()
        {
            _mat.SetFloat(Modify1, 1);
        }

        public void Destroy()
        {
            _mat.SetFloat(Destroy1, 1);
        }

        public void StopAction()
        {
            _mat.SetFloat(Create1, 0);
            _mat.SetFloat(Modify1, 0);
            _mat.SetFloat(Destroy1, 0);
        }

        public void CreateAction()
        {
            StopAction();
            _locked = true;
        }

        #endregion

        #region Selection

        public void OnSelect(SelectEnterEventArgs args)
        {
            if (!_isOwner)
            {
                if (_locked)
                    return;

                _isOwner = true;
                _locked = true;

                SendOwnership();
            }

            Interactors.Add(args.interactorObject);

            _moving = Interactors.Count == 1;
            _resizing = Interactors.Count == 2;

            Modify();

            if (_moving)
            {
                InitialDistance = Vector3.Distance(transform.position, Interactors[0].transform.position);
                gameObject.layer = _shapesLayer;
            }

            if (!_resizing) return;

            InitialDistance = Vector3.Distance(Interactors[0].transform.position, Interactors[1].transform.position);
            if (InitialDistance == 0)
                InitialDistance = 1;

            InitialScale = transform.localScale;
        }

        public void OnDeselect(SelectExitEventArgs args)
        {
            Interactors.Remove(args.interactorObject);

            _moving = Interactors.Count == 1;
            _resizing = false;

            if (_moving)
            {
                InitialDistance = Vector3.Distance(transform.position, Interactors[0].transform.position);
                gameObject.layer = _shapesLayer;

                return;
            }

            gameObject.layer = _defaultLayer;

            _isOwner = false;
            _locked = false;

            SendOwnership();
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

        public static void ReceiveNewObject(object[] data)
        {
            var position = (Vector3)data[0];
            var rotation = (Quaternion)data[1];
            var shapeId = (byte)data[2];

            var obj = Instantiate(Selector.GetShape(shapeId), position, rotation);
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

            Shapes[id].transform.position = position;
            Shapes[id].transform.rotation = rotation;
            Shapes[id].transform.localScale = scale;
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
            Shapes[id].Destroy();
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

        #endregion
    }
}