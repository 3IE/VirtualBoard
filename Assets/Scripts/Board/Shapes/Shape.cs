using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Board.Shapes
{
    public abstract class Shape : MonoBehaviour
    {
        protected bool Locked;
        protected bool IsOwner;

        protected float InitialDistance;
        protected Vector3 InitialScale;

        protected int ColliderId;

        private bool _moving;
        private bool _resizing;

        private Material _mat;

        protected List<IXRInteractor> Interactors;
        
        private static readonly int Create1 = Shader.PropertyToID("_Create");
        private static readonly int Modify1 = Shader.PropertyToID("_Modify");
        private static readonly int Destroy1 = Shader.PropertyToID("_Destroy");
        
        protected static int DefaultLayer;
        private static int _shapesLayer;

        #region Unity

        private void Start()
        {
            _mat = GetComponent<Renderer>().material;
            Interactors = new List<IXRInteractor>(2);
            ColliderId = GetComponent<Collider>().GetInstanceID();
            
            DefaultLayer = LayerMask.NameToLayer("Default");
            _shapesLayer = LayerMask.NameToLayer("Shapes");

            Create();

#if UNITY_EDITOR
            Interactors.Add(GameObject.Find("RightHand Controller").GetComponent<XRRayInteractor>());
            InitialDistance = 15;
            gameObject.layer = _shapesLayer;
            _moving = true;
#endif
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
            Locked = true;
        }

        #endregion

        #region Selection

        public void OnSelect(SelectEnterEventArgs args)
        {
            if (!IsOwner)
            {
                if (Locked)
                    return;
                
                IsOwner = true;
                Locked = true;

                //SendOwnership();
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

            gameObject.layer = DefaultLayer;
            
            IsOwner = false;
            Locked = false;
            
            //SendOwnership();
        }

        #endregion

        #region Networking

        /// <summary>
        /// Gets the new transform of the object from the server and applies it to the object.
        /// TODO: Implement
        /// </summary>
        public void UpdateTransform()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends the new transform of the object to the other clients
        /// TODO: Implement
        /// </summary>
        public void SendTransform()
        {
            if (IsOwner && Locked)
                ;
        }

        /// <summary>
        /// Tells the other clients to destroy this object
        /// TODO: Implement
        /// </summary>
        public void SendDestroy()
        {
            if (IsOwner && Locked)
                ;
        }

        /// <summary>
        /// Destroy this object
        /// TODO: Implement
        /// </summary>
        public void ReceiveDestroy()
        {
            Destroy();
        }

        /// <summary>
        /// Warns the other clients that this object can't be modified
        /// TODO: Implement 
        /// </summary>
        public void SendOwnership()
        {
        }

        /// <summary>
        /// Update this object so that it can't be modified until told otherwise
        /// TODO: Implement 
        /// </summary>
        public void UpdateOwnership()
        {
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