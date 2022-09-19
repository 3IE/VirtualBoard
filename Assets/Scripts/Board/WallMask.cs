using UnityEngine;

public class WallMask : MonoBehaviour
{
    [SerializeField]
    private Renderer[] renderers;
    [SerializeField]
    [Range(0, 20, order = 0)]
    private float softness = 3;
    [SerializeField]
    [Range(0, 20, order = 0)]
    private float radius = 15;

    private void Start()
    {
        foreach (Renderer r in renderers)
        {
            r.material.SetFloat("_Radius", radius);
            r.material.SetFloat("_Softness", softness);
            r.material.renderQueue = 3002;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        foreach (Renderer r in renderers)
            r.material.SetVector("_Position", transform.position);
    }
}
