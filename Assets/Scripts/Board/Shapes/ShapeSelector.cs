using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

namespace Board.Shapes
{
    public class ShapeSelector : MonoBehaviour
    {
        public const byte CubeId = 0;
        public const byte CylinderId = 0;
        public const byte SphereId = 0;

        [SerializeField] public XRRayInteractor leftInteractor;
        [SerializeField] private XRRayInteractor rightInteractor;

        [SerializeField] private InputActionReference createReference;
        [SerializeField] private InputActionReference destroyReference;
        [SerializeField] private XRInteractionManager interactionManager;
        [SerializeField] private Transform shapesParent;
        [SerializeField] private List<GameObject> shapes;

        private byte _index;

        [FormerlySerializedAs("_currentShape")] public Shape currentShape;
        private bool _creating;

        private void Awake()
        {
            createReference.action.started += CreateObject;
            createReference.action.canceled += StopCreateObject;

            destroyReference.action.started += DeleteObject;
            destroyReference.action.canceled += StopDeleteObject;
        }

        private void Start()
        {
            _index = CubeId;

            Shape.Selector = this;
        }

        public void SelectCube()
        {
            _index = CubeId;
        }

        public void SelectCylinder()
        {
            _index = CylinderId;
        }

        public void SelectSphere()
        {
            _index = SphereId;
        }

        public GameObject GetShape(byte shapeId)
        {
            return shapes[shapeId];
        }

        private GameObject GetShape()
        {
            return shapes[_index];
        }

        private void CreateObject(InputAction.CallbackContext ctx)
        {
            if (Shape.NumberOfShapes() >= 25)
                return;

            if (currentShape is not null && !currentShape.resizing)
            {
                currentShape.rotating = true;
                currentShape.moving = false;
                return;
            }

            _creating = true;

            var prefab = GetShape();
            var obj = Instantiate(prefab, shapesParent);
            obj.GetComponent<XRSimpleInteractable>().interactionManager = interactionManager;

            currentShape = obj.GetComponent<Shape>();
            currentShape.CreateAction(leftInteractor);
        }

        private void StopCreateObject(InputAction.CallbackContext ctx)
        {
            if (currentShape is null)
                return;
            
            if (currentShape.rotating)
            {
                currentShape.rotating = false;
                currentShape.moving = true;
                return;
            }
            
            _creating = false;
            
            currentShape.StopCreateAction(leftInteractor);
            currentShape = null;
        }

        private void DeleteObject(InputAction.CallbackContext ctx)
        {
            if (_creating)
                return;

            Shape.DeletionMode(true);

            if (Physics.Raycast(leftInteractor.transform.position, leftInteractor.transform.forward,
                    out var hit, 100f, LayerMask.GetMask("Static Shapes")))
                hit.collider.GetComponent<Shape>().Delete();
        }

        private void StopDeleteObject(InputAction.CallbackContext ctx)
        {
            if (_creating)
                return;

            Shape.DeletionMode(false);

            if (Physics.Raycast(leftInteractor.transform.position, leftInteractor.transform.forward,
                    out var hit, 100f, LayerMask.GetMask("Static Shapes")))
                hit.collider.GetComponent<Shape>().Destroy();
        }
    }
}