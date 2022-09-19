using System.Collections.Generic;
using UnityEngine;

public class ShapeSelector : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> shapes;

    private byte index;

    private void Start()
    {
        index = 0;
    }

    public void SelectCube()
    {
        index = 0;
    }

    public void SelectCylinder()
    {
        index = 1;
    }

    public void SelectSphere()
    {
        index = 2;
    }

    public GameObject GetShape()
    {
        return shapes[index];
    }
}
