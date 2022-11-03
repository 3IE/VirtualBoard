using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;
    private Vector3 _spawnPosition = Vector3.zero;
    // Update is called once per frame
    void Update()
    {
        Vector3 p = _spawnPosition - _prefab.transform.position;
        float   y = p.magnitude;
        transform.position = _spawnPosition - p.normalized * y / 2;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, y);
        
        transform.LookAt(_prefab.transform);
    }
}
