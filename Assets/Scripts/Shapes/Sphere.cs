using UnityEngine;

namespace Shapes
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

        /// <summary>
        ///     This object does not need to be rotated as it is a sphere.
        /// </summary>
        /// <remarks> Does nothing </remarks>
        protected override void Rotate()
        {
            // Do nothing
        }

        /// <inheritdoc />
        protected override bool CheckForCollision(Vector3 position)
        {
            return Physics.CheckSphere(position, _radius - 0.01f, _defaultPlayerMask);
        }

        /// <inheritdoc />
        protected override int CheckCast()
        {
            return Physics.SphereCastNonAlloc(Interactors[0].transform.position, _radius,
                                              Interactors[0].transform.forward, Hits, InitialDistance,
                                              _defaultMask);
        }

        /// <inheritdoc />
        protected override Vector3 GetPositionFromHit(RaycastHit hit)
        {
            return hit.point + hit.normal * _radius;
        }

        /// <inheritdoc />
        protected override void UpdateSize()
        {
            _radius = transform.localScale.x / 2;
        }
    }
}