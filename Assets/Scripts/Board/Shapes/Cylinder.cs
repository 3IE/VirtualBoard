namespace Board.Shapes
{
    /// <inheritdoc />
    public class Cylinder : Cube
    {
        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();
            
            ShapeId = ShapeSelector.CylinderId;

            Size.y *= 2;
        }

        private void Start()
        {
            Initialize();
        }
    }
}