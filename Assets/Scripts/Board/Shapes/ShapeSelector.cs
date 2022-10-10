using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Utils;

namespace Board.Shapes
{
    /// <summary>
    /// Singleton class, handles the input for the shapes.
    /// </summary>
    public class ShapeSelector : MonoBehaviour
    {
        /// <summary>
        /// Instance of the class
        /// </summary>
        public static ShapeSelector Instance { get; private set; }

        internal const byte CubeId = 0;
        internal const byte CylinderId = 1;
        internal const byte SphereId = 2;
        private const byte CustomShapeId = 3; // Find a way to determine id based on obj file

        /// <summary>
        /// shape currently held by the player
        /// </summary>
        public Shape currentShape;

        /// <summary>
        /// Left interactor
        /// </summary>
        [SerializeField] public XRRayInteractor leftInteractor;
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
            
            Instance = this;
        }

        private void Start()
        {
            _index = CubeId;
        }

        [SerializeField]
        private Material testMaterial;

        #region SELECTOR

        /// <summary>
        /// Sets the currently spawnable shape to be a cube
        /// </summary>
        public void SelectCube()
        {
            _index = CubeId;
        }

        /// <summary>
        /// Sets the currently spawnable shape to be a cylinder
        /// </summary>
        public void SelectCylinder()
        {
            _index = CylinderId;
        }

        /// <summary>
        /// Sets the currently spawnable shape to be a sphere
        /// </summary>
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

        /// <summary>
        /// Returns the shape corresponding to <c>shapeId</c>
        /// </summary>
        /// <param name="shapeId"> id of the shape we are searching for </param>
        /// <returns> corresponding shape </returns>
        public GameObject GetShape(byte shapeId)
        {
            return shapes[shapeId];
        }

        private GameObject GetShape()
        {
            if (_index < CustomShapeId) return shapes[_index];

#if DEBUG
            DebugPanel.Instance.AddCustom();
#endif

            return CustomShape.Create(_index, testMaterial);
        }

        #endregion

        private void CreateObject(InputAction.CallbackContext ctx)
        {
            if (Shape.NumberOfShapes() >= 25)
                return;

            if (currentShape is not null && !currentShape.Resizing)
            {
                currentShape.Rotating = true;
                currentShape.Moving = false;
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

            if (currentShape.Rotating)
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

            currentShape.InitialDistance =
                Mathf.Clamp(currentShape.InitialDistance + value * velocity, 0.5f, 100f);
        }

        /// <summary>
        /// Allows the user to change the distance between him and the object without moving
        /// </summary>
        public void StartChangeDistance()
        {
            continuousMoveProvider.enabled = false;
        }

        /// <summary>
        /// Removes the possibility to change the distance between the user and the object and allow him to move
        /// </summary>
        public void StopChangeDistance()
        {
            continuousMoveProvider.enabled = true;
        }
    }
}