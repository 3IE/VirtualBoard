using UnityEngine;

public class Appear : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    private Material mat;

    private void Start() => mat = GetComponent<Renderer>().material;

    // Update is called once per frame
    void Update()
    {
        mat.SetFloat("_Distance", Vector3.Distance(transform.position, player.transform.position));
    }
}
