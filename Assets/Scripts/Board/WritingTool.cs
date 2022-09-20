using UnityEngine;
using Utils;

namespace Board
{
    public class WritingTool : MonoBehaviour
    {
        [SerializeField]
        private HoverInteractable hover;

        public bool rotationLocked;

        private bool _touchedLast;
        private Quaternion _lastRot;
        private Vector3 _lastPosition;

        protected bool CanDraw;

        protected void UpdateRotation()
        {
            if (rotationLocked)
            {
                if (_touchedLast)
                {
                    transform.rotation = _lastRot;
                    
                    if (transform.position.z <= _lastPosition.z) return;
                    
                    _lastPosition.x = transform.position.x;
                    _lastPosition.y = transform.position.y;
                    transform.position = _lastPosition;
                }
                else
                {
                    _lastRot = transform.rotation;
                    _lastPosition = transform.position;
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
    }
}
