using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Board.Shapes
{
    public class CustomShape : Cube
    {
        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        public static CustomShape Create(Vector3 size, Vector3 position, Quaternion rotation, Material material)
        {
            var gameObject = new GameObject("CustomShape", typeof(MeshFilter), typeof(MeshRenderer), typeof(XRSimpleInteractable));
            var shape = gameObject.AddComponent<CustomShape>();
            shape._mesh = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f, -0.5f),
                    new Vector3(-0.5f, -0.5f, 0.5f),
                    new Vector3(-0.5f, 0.5f, -0.5f),
                    new Vector3(-0.5f, 0.5f, 0.5f),
                    new Vector3(0.5f, -0.5f, -0.5f),
                    new Vector3(0.5f, -0.5f, 0.5f),
                    new Vector3(0.5f, 0.5f, -0.5f),
                    new Vector3(0.5f, 0.5f, 0.5f)
                },
                triangles = new[]
                {
                    0, 1, 2,
                    1, 3, 2,
                    4, 6, 5,
                    5, 6, 7,
                    0, 2, 4,
                    4, 2, 6,
                    1, 5, 3,
                    5, 7, 3,
                    0, 4, 1,
                    4, 5, 1,
                    2, 3, 6,
                    6, 3, 7
                },
                colors = new[]
                {
                    Color.red,
                    Color.red,
                    Color.red,
                    Color.red,
                    Color.red,
                    Color.red,
                    Color.red,
                    Color.red,
                },
            };
            
            shape._meshFilter = gameObject.GetComponent<MeshFilter>();
            shape._meshRenderer = gameObject.GetComponent<MeshRenderer>();
            
            shape._mesh.RecalculateNormals();
            shape._mesh.RecalculateBounds();
            shape._mesh.Optimize();
            shape._meshFilter.mesh = shape._mesh;
            shape._meshRenderer.material = material;
            shape.transform.position = position;
            shape.transform.rotation = rotation;
            shape.transform.localScale = size;
            return shape;
        }
    }
}