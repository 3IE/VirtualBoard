namespace Shapes
{
    /// <inheritdoc />
    public class Cylinder : Cube
    {
        private void Start()
        {
            Initialize();
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            ShapeId = ShapeSelector.CylinderId;

            Size.y *= 2;
        }
    }
}