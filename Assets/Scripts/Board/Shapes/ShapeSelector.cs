using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Board.Shapes
{
    public class ShapeSelector : MonoBehaviour
    {
        public const byte CubeId = 0;
        public const byte CylinderId = 0;
        public const byte SphereId = 0;

        [SerializeField] private XRRayInteractor leftInteractor;
        [SerializeField] private XRRayInteractor rightInteractor;

        [SerializeField] private InputActionReference createReference;
        [SerializeField] private XRInteractionManager interactionManager;
        [SerializeField] private Transform shapesParent;
        [SerializeField] private List<GameObject> shapes;

        private byte _index;

        private Shape _currentShape;

        private void Awake()
        {
            createReference.action.started += CreateObject;
            createReference.action.canceled += StopCreateObject;
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
            var prefab = GetShape();
            var obj = Instantiate(prefab, shapesParent);
            obj.GetComponent<XRSimpleInteractable>().interactionManager = interactionManager;
            _currentShape = obj.GetComponent<Shape>();
            _currentShape.CreateAction(leftInteractor);
        }
        
        private void StopCreateObject(InputAction.CallbackContext ctx)
        {
            _currentShape.StopCreateAction(leftInteractor);
        }
    }
}