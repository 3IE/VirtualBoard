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

        private bool _moving;
        private bool _resizing;

        private Material _mat;

        protected List<IXRInteractor> Interactors;
        
        private static readonly int Create1 = Shader.PropertyToID("_Create");
        private static readonly int Modify1 = Shader.PropertyToID("_Modify");
        private static readonly int Destroy1 = Shader.PropertyToID("_Destroy");

        #region Unity

        private void Start()
        {
            _mat = GetComponent<Renderer>().material;
            Interactors = new List<IXRInteractor>(2);

            Create();
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

            if (_moving) return;
            
            IsOwner = false;
            Locked = false;
            
            //SendOwnership();
        }

#if UNITY_EDITOR

        public void OnSelect(HoverEnterEventArgs args)
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

            if (!_resizing) return;
            
            InitialDistance = Vector3.Distance(Interactors[0].transform.position, Interactors[1].transform.position);
            if (InitialDistance == 0)
                InitialDistance = 1;
            
            InitialScale = transform.localScale;
        }

        public void OnDeselect(HoverExitEventArgs args)
        {
            Interactors.Remove(args.interactorObject);

            _moving = Interactors.Count == 1;
            _resizing = false;

            if (_moving) return;
            
            IsOwner = false;
            Locked = false;
            
            //SendOwnership();
        }

#endif

        #endregion

        #region Networking

        public void UpdateTransform()
        {
            throw new NotImplementedException();
        }

        public void SendTransform()
        {
            if (IsOwner && Locked)
                throw new NotImplementedException();
        }

        public void SendDestroy()
        {
            if (IsOwner && Locked)
                throw new NotImplementedException();
        }

        public void ReceiveDestroy()
        {
            Destroy();
        }

        public void SendOwnership()
        {
            throw new NotImplementedException();
        }

        public void UpdateOwnership()
        {
            throw new NotImplementedException();
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