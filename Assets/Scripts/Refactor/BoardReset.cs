using UnityEngine;

namespace Refactor
{
    public class BoardReset : MonoBehaviour
    {
        [SerializeField] private Camera boardCamera;
    
        public void ResetBoard()
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = boardCamera.targetTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }
    }
}
