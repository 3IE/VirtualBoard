using JetBrains.Annotations;
using UnityEngine;
using Utils;

namespace Board
{
    public class WritingTool : MonoBehaviour
    {
        [SerializeField] private HoverInteractable hover;
        [SerializeField] protected Board boardObject;

        public bool rotationLocked;

        private bool _touchedLast;
        private Quaternion _lastRot;
        private Vector3 _lastPosition;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        
        private Transform _transform;
        private Rigidbody _rigidbody;

        protected bool CanDraw;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _transform = transform;
            
            _initialPosition = _transform.position;
            _initialRotation = _transform.rotation;
        }

        protected void UpdateRotation()
        {
            if (rotationLocked)
            {
                if (_touchedLast)
                {
                    transform.rotation = _lastRot;

                    if (transform.position.z <= _lastPosition.z) return;

                    var position = _transform.position;
                    
                    _lastPosition.x = position.x;
                    _lastPosition.y = position.y;
                    
                    _transform.position = _lastPosition;
                }
                else
                {
                    _lastRot = _transform.rotation;
                    _lastPosition = _transform.position;
                    _touchedLast = true;
                }
            }
            else
                _touchedLast = false;
        }
        
        public void AuthorizeDraw()
        {
            CanDraw = true;
            hover.HoverExit();
        }

        public void UnauthorizeDraw()
        {
            CanDraw = false;
            hover.Hover();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Ceiling")) return;

            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            _transform.position = _initialPosition;
            _transform.rotation = _initialRotation;

            _rigidbody.constraints = RigidbodyConstraints.None;
        }
    }
}