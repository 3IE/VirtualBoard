using UnityEngine;

namespace Board.Shapes
{
    /// <inheritdoc />
    public class Cube : Shape
    {
        /// <summary>
        /// size of the shape
        /// </summary>
        protected Vector3 Size = Vector3.one / 2;

        private int _defaultMask;
        private int _defaultPlayerMask;

        private RaycastHit[] _hits;
        
        private Transform _transform;

        /// <summary>
        /// Initialises some parameters of the shape
        /// </summary>
        /// <remarks> Is called through <see cref="Start"/> </remarks>
        protected virtual void Initialize()
        {
            _defaultMask = LayerMask.GetMask("Default", "Static Shapes");
            _defaultPlayerMask = LayerMask.GetMask("Default", "Player", "Static Shapes");
            _transform = transform;

            _hits = new RaycastHit[20];

            ShapeId = ShapeSelector.CubeId;
        }

        private void Start()
        {
            Initialize();
        }

        /// <inheritdoc />
        protected override void Move()
        {
            if (Physics.Raycast(Interactors[0].transform.position, Interactors[0].transform.forward,
                    out var hit, InitialDistance, _defaultMask))
            {
                var direction = new Vector3(hit.normal.x * Size.x, hit.normal.y * Size.y, hit.normal.z * Size.z);
                var position = hit.point + direction;
                
                var extents = new Vector3(Size.x - .001f, Size.y - .001f, Size.z - .001f);
                if (!Physics.CheckBox(position, extents, _transform.rotation, _defaultPlayerMask))
                {
                    _transform.position = position;
                    InitialDistance = hit.distance;
                    return;
                }
            }
            
            var size = Physics.BoxCastNonAlloc(Interactors[0].transform.position, Size,
                Interactors[0].transform.forward, _hits, _transform.rotation, InitialDistance, _defaultMask);
            var positionFound = false;

            for (var i = size - 1; i >= 0 && !positionFound; i--)
            {
                var direction = new Vector3(_hits[i].normal.x * Size.x, _hits[i].normal.y * Size.y, _hits[i].normal.z * Size.z);
                var position = _hits[i].point + direction;

                var extents = new Vector3(Size.x - .001f, Size.y - .001f, Size.z - .001f);
                if (Physics.CheckBox(position, extents, _transform.rotation, _defaultPlayerMask))
                    continue;

                _transform.position = position;
                //initialDistance = hit.distance;
                positionFound = true;
            }

            if (!positionFound)
                _transform.position = Interactors[0].transform.position +
                                     Interactors[0].transform.forward * InitialDistance;

            SendTransform();
        }

        /// <inheritdoc />
        protected override void Resize()
        {
            if (Interactors[0].transform.position == Interactors[1].transform.position)
                return;

            _transform.localScale =
                InitialScale / InitialDistance
                * Vector3.Distance(Interactors[0].transform.position, Interactors[1].transform.position);
            Size = _transform.localScale / 2;

            SendTransform();
        }

        /// <inheritdoc />
        protected override void Rotate()
        {
            _transform.rotation = Interactors[0].transform.rotation;
            
            SendTransform();
        }
    }
}