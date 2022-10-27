using UnityEngine;

namespace Board.Tools
{
    public class KinematicEraser : KinematicMarker
    {
        private Color _color;

        protected override void Initialize()
        {
            base.Initialize();

            _color = Board.Instance.texture.GetPixel(0, 0);
        }

        protected override Color[] GenerateShape()
        {
            return Tools.GenerateSquare(_color, penSize);
        }
    }
}