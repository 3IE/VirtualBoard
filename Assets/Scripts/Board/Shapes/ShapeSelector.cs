using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Board.Shapes
{
    public class ShapeSelector : MonoBehaviour
    {
        public static readonly byte CubeId = 0;
        public static readonly byte CylinderId = 0;
        public static readonly byte SphereId = 0;
        
        [SerializeField]
        private List<GameObject> shapes;

        [SerializeField]
        private XRRayInteractor leftInteractor;
        [SerializeField]
        private XRRayInteractor rightInteractor;

        private byte _index;

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

        public GameObject GetShape()
        {
            return shapes[_index];
        }

        public GameObject GetShape(byte shapeId)
        {
            return shapes[shapeId];
        }
    }
}
