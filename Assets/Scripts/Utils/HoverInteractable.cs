using UnityEngine;

public class HoverInteractable : MonoBehaviour
{
    [SerializeField]
    private Material mat;

    public void Hover()
    {
        mat.SetInt("_hovered", 1);
    }

    public void HoverExit()
    {
        mat.SetInt("_hovered", 0);
    }
}
