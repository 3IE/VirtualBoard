using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Utils;
using EventCode = Utils.EventCode;

namespace Board.Tools
{
    /// <summary>
    /// Class used to represent a modification done to the texture of the board
    /// </summary>
    public class Modification
    {
        /// <summary>
        /// x coordinate of the starting point
        /// </summary>
        public readonly int X;

        /// <summary>
        /// y coordinate of the starting point
        /// </summary>
        public readonly int Y;

        /// <summary>
        /// x coordinate of the destination point
        /// </summary>
        public readonly float DestX;

        /// <summary>
        /// y coordinate of the destination point
        /// </summary>
        public readonly float DestY;

        /// <summary>
        /// new color of the pixels
        /// </summary>
        public readonly Color Color;

        /// <summary>
        /// size of the square to modify
        /// </summary>
        public readonly float PenSize;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x"> x coordinate of the starting point </param>
        /// <param name="y"> y coordinate of the starting point </param>
        /// <param name="destX"> x coordinate of the destination point </param>
        /// <param name="destY"> y coordinate of the destination point </param>
        /// <param name="color"> new color of the pixels </param>
        /// <param name="penSize"> size of the square to modify </param>
        public Modification(int x, int y, float destX, float destY, Color color, float penSize)
        {
            X = x;
            Y = y;
            DestX = destX;
            DestY = destY;
            Color = color;
            PenSize = penSize;
        }

        /// <summary>
        /// Constructor used when receiving data from the network
        /// </summary>
        /// <param name="data"> object array holding the data used to create the object </param>
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

        internal void Send(EventCode code)
        {
            var colors = new[] { Color.r, Color.g, Color.b };

            object[] content = { X, Y, DestX, DestY, colors, PenSize };

            // We send the data to every other person in the room
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            // We send the event
            PhotonNetwork.RaiseEvent((byte)code, content, raiseEventOptions, SendOptions.SendReliable);

#if DEBUG
            DebugPanel.Instance.AddBoardSent();
#endif
        }
    }
}