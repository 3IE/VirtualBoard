using UnityEngine;
using Event = Utils.Event;

namespace Board.Tools
{
    public class Eraser : Marker
    {
        private Color _color;

        protected override void Initialize()
        {
            base.Initialize();
            
            _color = boardObject.texture.GetPixel(0, 0);
        }

        protected override void SendModification(int x, int y)
        {
            new Modification(x, y, LastTouchPos.x, LastTouchPos.y, _color,
                    penSize)
                .Send(Event.EventCode.Marker);
        }

        protected override Color[] GenerateShape()
        {
            return Tools.GenerateSquare(_color, penSize);
            // TODO generate shape depending on selected one
        }
    }
}