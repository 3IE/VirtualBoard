using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dummiesman;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Board.Shapes
{
    public class CustomShape : Cube
    {
        private const int CacheSize = 4;
        private static readonly List<KeyValuePair<byte, GameObject>> Cache = new(CacheSize);
        private static Dictionary<byte, string> _paths;

        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private static byte _id;

        protected override void Initialize()
        {
            base.Initialize();

            ShapeId = _id;
        }

        private void OnDestroy()
        {
            for (var i = 0; i < Cache.Count; i++)
            {
                if (Cache[i].Key != ShapeId) continue;
                if (!ReferenceEquals(Cache[i].Value, gameObject)) break;

                Cache.RemoveAt(i);

                var other = Shapes.First(pair => pair.Value.ShapeId == ShapeId).Value;
                if (other is not null)
                    Cache.Add(new KeyValuePair<byte, GameObject>(ShapeId, other.gameObject));

                break;
            }
        }

        public static void Init()
        {
            _paths = new Dictionary<byte, string>
            {
                //string[] files = Directory.GetFiles(Application.streamingAssetsPath, "*.obj");
                { 3, Path.GetFullPath("Assets/Models/CustomShape.obj") }
            };
        }

        public static GameObject Create(byte id, Material material)
        {
            _id = id;

            var cached = Cache.Find(pair => pair.Key == id).Value;
            if (cached is not null && !cached.IsDestroyed())
            {
                var instantiation = Instantiate(cached, Vector3.zero, Quaternion.identity);
                var instantiationInteractable = instantiation.GetComponent<XRSimpleInteractable>();
                var instantiationShape = instantiation.GetComponent<CustomShape>();

                instantiationInteractable.hoverEntered.AddListener(instantiationShape.OnHoverEnter);
                instantiationInteractable.hoverExited.AddListener(instantiationShape.OnHoverExit);
                instantiationInteractable.selectEntered.AddListener(instantiationShape.OnSelect);
                instantiationInteractable.selectExited.AddListener(instantiationShape.OnDeselect);

                instantiationShape.ShapeId = id;

                return instantiation;
            }

            var obj = new OBJLoader()
                .Load(_paths[id]);
            var mesh = obj.GetNamedChild("default");

            if (Cache.Count == CacheSize)
                Cache.RemoveAt(0);
            Cache.Add(new KeyValuePair<byte, GameObject>(id, mesh));

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
            shape.Mat = shape._meshRenderer.material;

            mesh.transform.localPosition = Vector3.zero;
            mesh.transform.localRotation = Quaternion.identity;
            mesh.transform.localScale /= shape._mesh.bounds.size.magnitude;

            return mesh;
        }
    }
}