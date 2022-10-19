using UnityEngine;
using Utils;

namespace Board.Tools
{
    /// <inheritdoc />
    public class Eraser : Marker
    {
        private Color _color;

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            _color = Board.Instance.texture.GetPixel(0, 0);
        }

        /// <inheritdoc />
        protected override void SendModification(int x, int y)
        {
            new Modification(x, y, LastTouchPos.x,
                             LastTouchPos.y, _color,
                             penSize)
                .Send(EventCode.Marker);
        }

        /// <inheritdoc />
        protected override Color[] GenerateShape()
        {
            return Tools.GenerateSquare(_color, penSize);

            // TODO generate shape depending on selected one
        }
    }
}