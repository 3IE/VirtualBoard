using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Board.Shapes
{
    public class ShapeSelector : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> shapes;

        [SerializeField]
        private XRRayInteractor leftInteractor;
        [SerializeField]
        private XRRayInteractor rightInteractor;

        private byte _index;

        private void Start()
        {
            _index = 0;
        }

        public void SelectCube()
        {
            _index = 0;
        }

        public void SelectCylinder()
        {
            _index = 1;
        }

        public void SelectSphere()
        {
            _index = 2;
        }

        public GameObject GetShape()
        {
            return shapes[_index];
        }
    }
}
