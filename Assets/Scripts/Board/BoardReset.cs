using System.Linq;
using Board.Events;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using EventCode = Utils.EventCode;

namespace Board
{
    /// <summary>
    ///     Utility class to reset the texture of the board
    /// </summary>
    public class BoardReset : MonoBehaviour
    {
        [SerializeField] private Board board;

        /// <summary>
        ///     Used to clean the board
        /// </summary>
        public void OnPush()
        {
            Invoke(nameof(ResetBoard), 0f);
        }

        private void ResetBoard()
        {
            Color   c   = Tools.Tools.Instance.baseColor;
            Color[] arr = board.texture.GetPixels();

            arr = Enumerable.Repeat(c, arr.Length).ToArray();

            board.texture.SetPixels(arr);
            board.texture.Apply();

            PlayerEvents.Clear();

            byte[] content = board.texture.EncodeToPNG();

            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte) EventCode.Texture, content, raiseEventOptions,
                                     SendOptions.SendReliable);
        }
    }
}