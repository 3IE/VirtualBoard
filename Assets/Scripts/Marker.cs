using System.Linq;
using UnityEngine;

public class Marker : MonoBehaviour
{
    [SerializeField]
    private Transform tip;
    [SerializeField] 
    private int penSize = 5;

    private Renderer renderer;
    private Color[] colors;
    private float tipHeight;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();
        colors = Enumerable.Repeat(renderer.material.color, penSize * penSize).ToArray();
        tipHeight = tip.localScale.y;
    }

    private void Update()
    {
        
    }

    /// <summary>
    /// Check if we are touching the board
    /// if we are not, return
    /// otherwise draw while touching (interpolation used to avoid drawing only dots)
    /// when stop touching, send new texture to ar clients
    /// </summary>
    private void Draw()
    {
        //TODO
    }

    /// <summary>
    /// Equivalent of the Draw method but with the base color of the board
    /// </summary>
    private void Erase()
    {
        // TODO
    }

    // TODO add other shapes ?
}
