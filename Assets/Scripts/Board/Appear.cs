using UnityEngine;

namespace Board
{
    public class Appear : MonoBehaviour
    {
        [SerializeField]
        private GameObject player;
        private Material _mat;
    
        private static readonly int Distance = Shader.PropertyToID("_Distance");

        private void Start() => _mat = GetComponent<Renderer>().material;

        // Update is called once per frame
        private void Update()
        {
            _mat.SetFloat(Distance, Vector3.Distance(transform.position, player.transform.position));
        }
    }
}
