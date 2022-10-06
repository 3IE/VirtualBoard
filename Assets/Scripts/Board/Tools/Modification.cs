using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Event = Utils.Event;

namespace Board.Tools
{
    public class Modification
    {
        public readonly int X;
        public readonly int Y;
        public readonly float DestX;
        public readonly float DestY;
        public readonly Color Color;
        public readonly float PenSize;

        public Modification(int x, int y, float destX, float destY, Color color, float penSize)
        {
            X = x;
            Y = y;
            DestX = destX;
            DestY = destY;
            Color = color;
            PenSize = penSize;
        }

        public Modification(object data)
        {
            var dataArray = (object[])data;
            var colors = (float[])dataArray[4];

            X = (int)dataArray[0];
            Y = (int)dataArray[1];
            DestX = (float)dataArray[2];
            DestY = (float)dataArray[3];
            Color = new Color(colors[0], colors[1], colors[2]);
            PenSize = (float)dataArray[5];
        }

        internal void Send(Event.EventCode code)
        {
            var colors = new[] {Color.r, Color.g, Color.b};

            object[] content = { X, Y, DestX, DestY, colors, PenSize };

            // We send the data to every other person in the room
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            // We send the event
            PhotonNetwork.RaiseEvent((byte)code, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }
}