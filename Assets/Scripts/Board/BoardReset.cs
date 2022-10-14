using System.Linq;
using Board.Events;
using UnityEngine;

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
            Color   c   = Tools.Tools.Instance.baseColor;
            Color[] arr = board.texture.GetPixels();

            arr = Enumerable.Repeat(c, arr.Length).ToArray();

            board.texture.SetPixels(arr);
            board.texture.Apply();

            PlayerEvents.Clear();
        }
    }
}