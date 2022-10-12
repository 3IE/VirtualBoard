namespace Board.Shapes
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
            base.Initialize();

            ShapeId = ShapeSelector.CylinderId;

            Size.y *= 2;
        }
    }
}