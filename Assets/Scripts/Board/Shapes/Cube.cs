using UnityEngine;

namespace Board.Shapes
{
    /// <inheritdoc />
    public class Cube : Shape
    {
        /// <summary>
        ///     size of the shape
        /// </summary>
        protected Vector3 Size = Vector3.one / 2;

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        ///     Initialises some parameters of the shape
        /// </summary>
        /// <remarks> Is called through <see cref="Start" /> </remarks>
        protected virtual void Initialize()
        {
            ShapeId = ShapeSelector.CubeId;
        }

        /// <inheritdoc />
        protected override bool CheckForCollision(Vector3 position)
        {
            var extents = new Vector3(Size.x - .001f, Size.y - .001f, Size.z - .001f);

            return Physics.CheckBox(position, extents, Transform.rotation,
                                    DefaultPlayerMask);
        }

        /// <inheritdoc />
        protected override int CheckCast()
        {
            return Physics.BoxCastNonAlloc(Interactors[0].transform.position, Size,
                                           Interactors[0].transform.forward, Hits, Transform.rotation,
                                           InitialDistance, DefaultMask);
        }

        /// <inheritdoc />
        protected override Vector3 GetPositionFromHit(RaycastHit hit)
        {
            var direction = new Vector3(hit.normal.x * Size.x, hit.normal.y * Size.y,
                                        hit.normal.z * Size.z);
            return hit.point + direction;
        }

        /// <inheritdoc />
        protected override void UpdateSize()
        {
            Size = Transform.localScale / 2;
        }
    }
}