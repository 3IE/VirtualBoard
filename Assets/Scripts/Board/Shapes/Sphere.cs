public class Sphere : Shape
{
    protected override void Move()
    {
        throw new System.NotImplementedException();
    }

    protected override void Resize()
    {
        transform.localScale = initialScale * UnityEngine.Vector3.Distance(transformLeft.position, transformRight.position) / initialDistance;
    }
}
