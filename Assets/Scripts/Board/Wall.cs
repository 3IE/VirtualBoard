using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    [SerializeField]
    private Vector2 position;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material.SetVector("_Tile", position);
    }
}
