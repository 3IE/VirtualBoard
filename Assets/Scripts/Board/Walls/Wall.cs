using UnityEngine;

namespace Board
{
    public class Wall : MonoBehaviour
    {
        [SerializeField]
        private Vector2 position;

        private static readonly int Tile = Shader.PropertyToID("_Tile");

        // Start is called before the first frame update
        private void Start()
        {
            GetComponent<Renderer>().material.SetVector(Tile, position);
        }
    }
}
