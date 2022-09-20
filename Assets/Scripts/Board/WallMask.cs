using UnityEngine;

namespace Board
{
    public class WallMask : MonoBehaviour
    {
        [SerializeField]
        private Renderer[] renderers;
        [SerializeField]
        [Range(0, 20, order = 0)]
        private float softness = 3;
        [SerializeField]
        [Range(0, 20, order = 0)]
        private float radius = 15;

        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int Softness = Shader.PropertyToID("_Softness");
        private static readonly int Position = Shader.PropertyToID("_Position");

        private void Start()
        {
            foreach (Renderer r in renderers)
            {
                r.material.SetFloat(Radius, radius);
                r.material.SetFloat(Softness, softness);
                r.material.renderQueue = 3002;
            }
        }

        // Update is called once per frame
        private void Update()
        {
            foreach (Renderer r in renderers)
                r.material.SetVector(Position, transform.position);
        }
    }
}
