namespace Board.Shapes
{
    public class Sphere : Shape
    {
        protected override void Move()
        {
            throw new System.NotImplementedException();
        }

        protected override void Resize()
        {
            if (Interactors[0].transform.position == Interactors[1].transform.position)
                return;
            
            transform.localScale = InitialScale
                                   * UnityEngine.Vector3.Distance(Interactors[0].transform.position, Interactors[1].transform.position)
                                   / InitialDistance;
        }
    }
}