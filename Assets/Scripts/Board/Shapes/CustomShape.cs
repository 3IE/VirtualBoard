using Dummiesman;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Board.Shapes
{
    public class CustomShape : Cube
    {
        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        public static GameObject Create(Material material)
        {
            var obj = new OBJLoader()
                .Load("C:\\Users\\yvon.morice\\Documents\\VirtualBoard\\Assets\\Models\\CustomShape.obj");
            var mesh = obj.GetNamedChild("default");

            var collider = mesh.AddComponent<MeshCollider>();
            var interactable = mesh.AddComponent<XRSimpleInteractable>();
            var rigidbody = mesh.AddComponent<Rigidbody>();

            rigidbody.useGravity = false;

            collider.convex = true;
            
            var shape = mesh.AddComponent<CustomShape>();
            
            interactable.hoverEntered.AddListener(shape.OnHoverEnter);
            interactable.hoverExited.AddListener(shape.OnHoverExit);
            interactable.selectEntered.AddListener(shape.OnSelect);
            interactable.selectExited.AddListener(shape.OnDeselect);

            interactable.selectMode = InteractableSelectMode.Multiple;

            shape._meshFilter = mesh.GetComponent<MeshFilter>();
            shape._meshRenderer = mesh.GetComponent<MeshRenderer>();

            shape._mesh = shape._meshFilter.mesh;

            shape._meshRenderer.material = material;

            mesh.transform.localPosition = Vector3.zero;
            mesh.transform.localRotation = Quaternion.identity;
            mesh.transform.localScale /= shape._mesh.bounds.size.magnitude;
            return mesh;
        }
    }
}