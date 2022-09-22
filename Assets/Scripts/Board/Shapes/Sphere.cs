using UnityEngine;

namespace Board.Shapes
{
    public class Sphere : Shape
    {
        private float _radius = 0.5f;

        private int _defaultMask;
        private int _defaultPlayerMask;

        private RaycastHit[] _hits;

        private void Start()
        {
            _defaultMask = LayerMask.GetMask("Default");
            _defaultPlayerMask = LayerMask.GetMask("Default", "Player");

            _hits = new RaycastHit[20];

            ShapeId = ShapeSelector.SphereId;
            
            SendNewObject();
        }

        protected override void Move()
        {
            if (Physics.Raycast(Interactors[0].transform.position, Interactors[0].transform.forward,
                    out var hit, 100f, _defaultMask))
            {
                Vector3 position = hit.point + hit.normal * _radius;
                if (!Physics.CheckSphere(position, _radius - 0.01f, _defaultPlayerMask))
                {
                    transform.position = position;
                    return;
                }
            }

            var size = Physics.SphereCastNonAlloc(Interactors[0].transform.position, _radius,
                Interactors[0].transform.forward, _hits, 100f, _defaultMask);
            var positionFound = false;

            for (int i = size - 1; i >= 0 && !positionFound; i--)
            {
                Vector3 position = _hits[i].point + _hits[i].normal * _radius;

                if (Physics.CheckSphere(position, _radius - 0.01f, _defaultPlayerMask))
                    continue;

                transform.position = position;
                positionFound = true;
            }

            if (!positionFound)
                transform.position = Interactors[0].transform.position +
                                     Interactors[0].transform.forward * InitialDistance;

            SendTransform();
        }

        protected override void Resize()
        {
            if (Interactors[0].transform.position == Interactors[1].transform.position)
                return;

            transform.localScale =
                InitialScale / InitialDistance
                * Vector3.Distance(Interactors[0].transform.position, Interactors[1].transform.position);
            _radius = transform.localScale.x / 2;

            SendTransform();
        }

        /// <summary>
        /// This object does not need to be rotated as it is a sphere.
        /// </summary>
        protected override void Rotate()
        {}
    }
}