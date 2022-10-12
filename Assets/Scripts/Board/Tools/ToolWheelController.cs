using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Board.Tools
{
    /// <summary>
    ///     Utility class used to select a tool
    /// </summary>
    /// <remarks> DO NOT USE </remarks>
    public class ToolWheelController : MonoBehaviour
    {
        private static readonly int Hover = Animator.StringToHash("Hover");

        [SerializeField] private int id;

        [SerializeField] private string item;

        [SerializeField] private bool selected;

        [SerializeField] private Animator anim;

        [SerializeField] private TextMeshProUGUI itemText;

        [SerializeField] private Image selectedItem;

        [SerializeField] private Sprite icon;

        private bool _hovered;

        // Start is called before the first frame update
        private void Start()
        {
            anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (selected)
            {
                if (!_hovered)
                    HoverEnter();
            }
            else if (_hovered)
                HoverExit();
        }

        private void HoverEnter()
        {
            anim.SetBool(Hover, true);
            selectedItem.sprite = icon;
            itemText.text       = item;
            _hovered            = true;
        }

        private void HoverExit()
        {
            anim.SetBool(Hover, false);
            _hovered = false;
        }
    }
}