using UnityEngine;

namespace Board.Shapes
{
    /// <summary>
    ///     Utility class used to reset the texture of the board
    /// </summary>
    public class ShapeReset : MonoBehaviour
    {
        /// <summary>
        ///     Called when the button is pushed
        /// </summary>
        public void OnPush()
        {
            foreach (Shape shape in Shape.Shapes.Values)
                shape.CallDestroy(false);

            Shape.Shapes.Clear();
        }
    }
}