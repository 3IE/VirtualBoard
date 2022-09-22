using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Board.Shapes
{
    public class Cube : Shape
    {
        private Vector3 _size = Vector3.one / 2;

        private int _defaultMask;
        private int _defaultPlayerMask;

        private RaycastHit[] _hits;
        
        private Transform _transform;

        private void Start()
        {
            _defaultMask = LayerMask.GetMask("Default");
            _defaultPlayerMask = LayerMask.GetMask("Default", "Player");
            _transform = transform;

            _hits = new RaycastHit[20];

            ShapeId = ShapeSelector.SphereId;
            
            SendNewObject();
        }

        protected override void Move()
        {
            Rotate();
            return; //TODO: remove this line to enable movement
            
            if (Physics.Raycast(Interactors[0].transform.position, Interactors[0].transform.forward,
                    out var hit, InitialDistance, _defaultMask))
            {
                var direction = new Vector3(hit.normal.x * _size.x, hit.normal.y * _size.y, hit.normal.z * _size.z);
                var position = hit.point + direction;
                
                var extents = new Vector3(_size.x - .001f, _size.y - .001f, _size.z - .001f);
                if (!Physics.CheckBox(position, extents, _transform.rotation, _defaultPlayerMask))
                {
                    _transform.position = position;
                    return;
                }
            }

            var size = Physics.BoxCastNonAlloc(Interactors[0].transform.position, _size,
                Interactors[0].transform.forward, _hits, _transform.rotation, InitialDistance, _defaultMask);
            var positionFound = false;

            for (int i = size - 1; i >= 0 && !positionFound; i--)
            {
                var direction = new Vector3(_hits[i].normal.x * _size.x, _hits[i].normal.y * _size.y, _hits[i].normal.z * _size.z);
                var position = _hits[i].point + direction;

                var extents = new Vector3(_size.x - .001f, _size.y - .001f, _size.z - .001f);
                if (Physics.CheckBox(position, extents, _transform.rotation, _defaultPlayerMask))
                    continue;

                _transform.position = position;
                positionFound = true;
            }

            if (!positionFound)
                _transform.position = Interactors[0].transform.position +
                                     Interactors[0].transform.forward * InitialDistance;

            SendTransform();
        }

        protected override void Resize()
        {
            if (Interactors[0].transform.position == Interactors[1].transform.position)
                return;

            _transform.localScale =
                InitialScale / InitialDistance
                * Vector3.Distance(Interactors[0].transform.position, Interactors[1].transform.position);
            _size = _transform.localScale / 2;

            SendTransform();
        }

        protected override void Rotate()
        {
            _transform.rotation = Interactors[0].transform.rotation * InitialRotation;
        }
    }
}