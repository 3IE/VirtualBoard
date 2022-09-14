using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolWheelController : MonoBehaviour
{
    [SerializeField]
    private int id;
    [SerializeField]
    private string item;
    [SerializeField]
    private bool selected = false;
    
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private TextMeshProUGUI itemText;
    [SerializeField]
    private Image selectedItem;
    [SerializeField]
    private Sprite icon;

    public bool hovered = false;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (selected)
        {
            if (!hovered)
                HoverEnter();
        }
        else if (hovered)
            HoverExit();

    }

    void HoverEnter()
    {
        anim.SetBool("Hover", true);
        selectedItem.sprite = icon;
        itemText.text = item;
        hovered = true;
    }

    void HoverExit()
    {
        anim.SetBool("Hover", false);
        hovered = false;
    }
}
