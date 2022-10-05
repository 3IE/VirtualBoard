using UnityEngine;
using Utils;

namespace Board.Tools
{
    public class WritingTool : MonoBehaviour
    {
        [SerializeField] private HoverInteractable hover;
        [SerializeField] protected Board boardObject;

        public bool rotationLocked;

        private Quaternion _lastRot;
        private Vector3 _lastPosition;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        
        private Transform _transform;
        private Rigidbody _rigidbody;

        #if UNITY_EDITOR
        [SerializeField]
        #endif
        protected bool canDraw;
        protected bool TouchedLast;

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
                if (TouchedLast)
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
                    TouchedLast = true;
                }
            }
            else
                TouchedLast = false;
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
                rotationLocked = true;
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

            TouchedLast = false;
            
            rotationLocked = false;
            canDraw = false;
        }
    }
}