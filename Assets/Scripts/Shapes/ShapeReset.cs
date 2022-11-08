using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shapes
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
            Invoke(nameof(ResetShapes), 0f);
        }

        private void ResetShapes()
        {
            List<Shape> shapes = Shape.Shapes.Values.ToList();

            foreach (Shape shape in shapes)
                shape.CallDestroy(true);

            Shape.Shapes.Clear();
        }
    }
}