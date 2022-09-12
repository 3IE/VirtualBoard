using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Modification
{
    public int x;
    public int y;
    public float destX;
    public float destY;
    public Color[] colors;
    public int penSize;

    public Modification(int x, int y, float destX, float destY, Color[] colors, int penSize)
    {
        this.x = x;
        this.y = y;
        this.destX = destX;
        this.destY = destY;
        this.colors = colors;
        this.penSize = penSize;
    }

    internal void Send(Event.EventCode code)
    {
        object[] content = new object[] { this };

        // We send the data to every other person in the room
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

        // We send the event
        PhotonNetwork.RaiseEvent((byte) code, content, raiseEventOptions, SendOptions.SendReliable);
    }
}