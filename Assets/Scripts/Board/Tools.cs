using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine;

public class Tools : MonoBehaviour
{
    [Tooltip("How many pixels we want to change around the tip of the tool")]
    public int penSize = 5;

    [Tooltip("How much we want to interpolate the draw between two frames (lower values means more data is covered)")]
    [Range(0.01f, 1.00f)]
    public float coverage = 0.01f;

    public Color baseColor;

    #region EVENT_CODES

    public const byte SendNewTextureEventCode = 1;

    #endregion

    public static Color[] generateSquare(Color color, Board board)
    {
        return Enumerable.Repeat(color, board.tools.penSize * board.tools.penSize).ToArray();
    }

    public static Color[] generateCircle(int x, int y, Color color, Board board)
    {
        Color[] colors = board.texture.GetPixels(x, y, board.tools.penSize, board.tools.penSize);

        Vector2Int center = new Vector2Int(board.tools.penSize / 2, board.tools.penSize / 2);

        for (int i = 0; i < board.tools.penSize * board.tools.penSize; i++)
        {
            Vector2Int pos = new Vector2Int(i % board.tools.penSize, i / board.tools.penSize);

            Vector2Int dist = new Vector2Int(pos.x - center.x, pos.y - center.y);
            float distance = Mathf.Sqrt(dist.x * dist.x + dist.y * dist.y);

            if (distance <= board.tools.penSize / 2)
                colors[i] = color;
        }

        return colors;
    }

    public static void SendNewTextureEvent(Board board)
    {
        // We send the whole texture
        object[] content = new object[] { board.texture };

        // We send the data to every other person in the room
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

        // We send the event
        PhotonNetwork.RaiseEvent(SendNewTextureEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }
}
