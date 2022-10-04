using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Board.Tools
{
    public class WritingTool : MonoBehaviour
    {
        [SerializeField] private HoverInteractable hover;
        [SerializeField] protected Board boardObject;

        [FormerlySerializedAs("rotationLocked")] public bool positionLocked;

        private bool _touchedLast;
        private Vector3 _lastPosition;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        
        private Transform _transform;
        private Rigidbody _rigidbody;

        #if UNITY_EDITOR
        [FormerlySerializedAs("CanDraw")] [SerializeField]
        #endif
        protected bool canDraw;

        protected Vector3 collisionPoint;
        protected Vector3 collisionNormal;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _transform = transform;
            
            _initialPosition = _transform.position;
            _initialRotation = _transform.rotation;
        }

        protected void UpdateRotation()
        {
            if (positionLocked)
            {
                if (_touchedLast)
                {
                    if (transform.position.z <= _lastPosition.z) return;

                    var position = _transform.position;
                    
                    _lastPosition.x = position.x;
                    _lastPosition.y = position.y;
                }
                else
                {
                    _lastPosition = _transform.position;
                    _touchedLast = true;
                }
            }
            else
                _touchedLast = false;
        }

        public void AuthorizeDraw()
        {
            hover.HoverExit();
        }

        public void UnauthorizeDraw()
        {
            hover.Hover();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Board"))
            {
                positionLocked = true;
                canDraw = true;
                return;
            }
            
            if (!other.CompareTag("Ceiling")) return;

            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            _transform.position = _initialPosition;
            _transform.rotation = _initialRotation;

            _rigidbody.constraints = RigidbodyConstraints.None;
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Board")) return;
            
            positionLocked = false;
            canDraw = false;
        }
    }
}