using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Event = Utils.Event;

namespace Board
{
    public class Modification
    {
        public readonly int X;
        public readonly int Y;
        public readonly float DestX;
        public readonly float DestY;
        public readonly Color[] Colors;
        public readonly int PenSize;

        public Modification(int x, int y, float destX, float destY, Color[] colors, int penSize)
        {
            X = x;
            Y = y;
            DestX = destX;
            DestY = destY;
            Colors = colors;
            PenSize = penSize;
        }

        internal void Send(Event.EventCode code)
        {
            // We send the data to every other person in the room
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            // We send the event
            PhotonNetwork.RaiseEvent((byte) code, this, raiseEventOptions, SendOptions.SendReliable);
        }
    }
}