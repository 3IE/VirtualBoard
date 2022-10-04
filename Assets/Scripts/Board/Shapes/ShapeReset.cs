using UnityEngine;

namespace Board.Shapes
{
    public class ShapeReset : MonoBehaviour
    {
        public void OnPush()
        {
            foreach (var shape in Shape.Shapes.Values)
                shape.CallDestroy(false);
        
            Shape.Shapes.Clear();
        }
    }
}
