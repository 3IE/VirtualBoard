using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Board
{
    public class ToolWheelController : MonoBehaviour
    {
        [SerializeField]
        private int id;
        [SerializeField]
        private string item;
        [SerializeField]
        private bool selected;
    
        [SerializeField]
        private Animator anim;
        [SerializeField]
        private TextMeshProUGUI itemText;
        [SerializeField]
        private Image selectedItem;
        [SerializeField]
        private Sprite icon;

        public bool hovered;
        private static readonly int Hover = Animator.StringToHash("Hover");

        // Start is called before the first frame update
        private void Start()
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

        private void HoverEnter()
        {
            anim.SetBool(Hover, true);
            selectedItem.sprite = icon;
            itemText.text = item;
            hovered = true;
        }

        private void HoverExit()
        {
            anim.SetBool(Hover, false);
            hovered = false;
        }
    }
}
