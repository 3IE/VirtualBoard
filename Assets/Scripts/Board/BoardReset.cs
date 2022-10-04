using System.Linq;
using UnityEngine;

public class BoardReset : MonoBehaviour
{
    [SerializeField] private Board.Board board;

    public void OnPush()
    {
        var c = board.tools.baseColor;
        var arr = board.texture.GetPixels();
        
        arr = Enumerable.Repeat(c, arr.Length).ToArray();
        
        board.texture.SetPixels(arr);
    }
}
