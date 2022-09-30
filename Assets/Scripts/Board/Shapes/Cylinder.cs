namespace Board.Shapes
{
    public class Cylinder : Cube
    {
        protected override void Initialize()
        {
            base.Initialize();
            
            Size.y *= 2;
        }

        private void Start()
        {
            Initialize();
        }
    }
}