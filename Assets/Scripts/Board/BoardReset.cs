using System.Linq;
using Board.Events;
using UnityEngine;

namespace Board
{
    /// <summary>
    /// Utility class to reset the texture of the board
    /// </summary>
    public class BoardReset : MonoBehaviour
    {
        [SerializeField] private Board board;

        /// <summary>
        /// Used to clean the board
        /// </summary>
        public void OnPush()
        {
            var c = Tools.Tools.Instance.baseColor;
            var arr = board.texture.GetPixels();
        
            arr = Enumerable.Repeat(c, arr.Length).ToArray();
        
            board.texture.SetPixels(arr);
            
            PlayerEvents.Clear();
        }
    }
}
