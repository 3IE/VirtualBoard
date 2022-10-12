using UnityEngine;

namespace Board.Shapes
{
    /// <inheritdoc />
    public class Sphere : Shape
    {
        private int _defaultMask;
        private int _defaultPlayerMask;

        private RaycastHit[] _hits;
        private float        _radius = 0.5f;

        private void Start()
        {
            _defaultMask       = LayerMask.GetMask("Default", "Static Shapes");
            _defaultPlayerMask = LayerMask.GetMask("Default", "Player", "Static Shapes");

            _hits = new RaycastHit[20];

            ShapeId = ShapeSelector.SphereId;
        }

        /// <inheritdoc />
        protected override void Move()
        {
            if (Physics.Raycast(Interactors[0].transform.position, Interactors[0].transform.forward,
                                out RaycastHit hit, InitialDistance, _defaultMask))
            {
                Vector3 position = hit.point + hit.normal * _radius;

                if (!Physics.CheckSphere(position, _radius - 0.01f, _defaultPlayerMask))
                {
                    transform.position = position;
                    InitialDistance    = hit.distance;
                    return;
                }
            }

            int size = Physics.SphereCastNonAlloc(Interactors[0].transform.position, _radius,
                                                  Interactors[0].transform.forward, _hits, InitialDistance,
                                                  _defaultMask);

            var positionFound = false;

            for (int i = size - 1; i >= 0 && !positionFound; i--)
            {
                Vector3 position = _hits[i].point + _hits[i].normal * _radius;

                if (Physics.CheckSphere(position, _radius - 0.01f, _defaultPlayerMask))
                    continue;

                transform.position = position;

                //initialDistance = hit.distance;
                positionFound = true;
            }

            if (!positionFound)
                transform.position = Interactors[0].transform.position
                                     + Interactors[0].transform.forward * InitialDistance;

            SendTransform();
        }

        /// <inheritdoc />
        protected override void Resize()
        {
            if (Interactors[0].transform.position == Interactors[1].transform.position)
                return;

            transform.localScale =
                InitialScale
                / InitialDistance
                * Vector3.Distance(Interactors[0].transform.position, Interactors[1].transform.position);

            _radius = transform.localScale.x / 2;

            SendTransform();
        }

        /// <summary>
        ///     This object does not need to be rotated as it is a sphere.
        /// </summary>
        /// <remarks> Does nothing </remarks>
        protected override void Rotate()
        {
            // Do nothing
        }
    }
}