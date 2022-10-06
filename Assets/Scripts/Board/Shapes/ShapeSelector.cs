using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Board.Shapes
{
    public class ShapeSelector : MonoBehaviour
    {
        public const byte CubeId = 0;
        public const byte CylinderId = 1;
        public const byte SphereId = 2;
        public const byte CustomShapeId = 3; // Find a way to determine id based on obj file

        public bool selected;
        public Shape currentShape;

        [SerializeField] public XRRayInteractor leftInteractor;
        [SerializeField] private XRRayInteractor rightInteractor;
        [SerializeField] private XRInteractionManager interactionManager;

        [SerializeField] private InputActionReference createReference;
        [SerializeField] private InputActionReference destroyReference;
        [SerializeField] private InputActionReference changeDistance;
        [SerializeField] private ContinuousMoveProviderBase continuousMoveProvider;

        [SerializeField] private Transform shapesParent;
        [SerializeField] private List<GameObject> shapes;

        [Range(0.5f, 5f)] [SerializeField] private float velocity = 0.5f;

        private byte _index;

        private bool _creating;

        private void Awake()
        {
            createReference.action.started += CreateObject;
            createReference.action.canceled += StopCreateObject;

            destroyReference.action.started += DeleteObject;
            destroyReference.action.canceled += StopDeleteObject;

            changeDistance.action.performed += ChangeDistance;
        }

        private void Start()
        {
            _index = CubeId;

            Shape.Selector = this;
        }

        public Material testMaterial;
#if UNITY_EDITOR
        public bool test;

        private void Update()
        {
            if (test)
                CreateObject();
            test = false;
        }

        private void CreateObject()
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
#endif

        #region SELECTOR

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

        /// <summary>
        /// Selects a custom shape.
        /// </summary>
        /// TODO: Find a way to select custom shape via UI
        /// TODO: Find a way to determine id based on obj file (load at start and attributes at that time?)
        public void SelectCustomShape()
        {
            _index = CustomShapeId;
        }

        public GameObject GetShape(byte shapeId)
        {
            return shapes[shapeId];
        }

        private GameObject GetShape()
        {
            return _index >= CustomShapeId
                ? CustomShape.Create(_index, testMaterial)
                : shapes[_index];
        }

        #endregion

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

            GameObject obj;
            if (_index >= CustomShapeId)
            {
                obj = prefab 
                    ? prefab
                    : throw new System.ArgumentNullException(nameof(prefab));
                obj.transform.SetParent(shapesParent);
            }
            else
                obj = Instantiate(prefab, shapesParent);

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
                currentShape.OnDeselect(null);
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
                hit.collider.GetComponent<Shape>().CallDestroy(false);
        }

        private void ChangeDistance(InputAction.CallbackContext obj)
        {
            if (currentShape is null)
                return;

            var value = obj.ReadValue<Vector2>().y * Time.deltaTime;

            currentShape.initialDistance =
                Mathf.Clamp(currentShape.initialDistance + value * velocity, 0.5f, 100f);
        }

        public void StartChangeDistance()
        {
            continuousMoveProvider.enabled = false;
        }

        public void StopChangeDistance()
        {
            continuousMoveProvider.enabled = true;
        }
    }
}