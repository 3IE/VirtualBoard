using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Utils;

namespace UI
{
    public class VRMenu : MonoBehaviour
    {
        // List of MenuPanels with their respective buttons
        [SerializeField] private List<GameObject> panels;
        [SerializeField] private List<GameObject> panelsButtons;
       
        [SerializeField] private float throwThreshold;
        [SerializeField] private float timeToFade;
        
        [SerializeField] private XRGrabInteractable grabInteractable;
        
        private PanelIndex _activePanelIndex = PanelIndex.PlayerList;

        private Rigidbody _rigidbody;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void ThrowAway() => StartCoroutine(ThrowAwayRoutine());

        private IEnumerator ThrowAwayRoutine()
        {
            grabInteractable.enabled = false;
            
            PrintVar.Print(1, "Destroying");

            if (_rigidbody.velocity.magnitude < throwThreshold) yield break;

            var i = 0.0f;

            while (i < timeToFade)
            {
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, i / timeToFade);
                i += Time.fixedDeltaTime;

                yield return null;
            }
            
            gameObject.SetActive(false);
        }

        public void WakeUp()
        {
            StopAllCoroutines();
            
            grabInteractable.enabled = true;

            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            _rigidbody.constraints = RigidbodyConstraints.None;
            
            _canvasGroup.alpha = 1;
        }

        public void SwitchPanel(int index) => SwitchPanel((PanelIndex)index);

        private void SwitchPanel(PanelIndex index)
        {
            panels[(int)_activePanelIndex].SetActive(false);
            panelsButtons[(int)_activePanelIndex].SetActive(true);
          
            _activePanelIndex = index;
            
            panels[(int)_activePanelIndex].SetActive(true);
            panelsButtons[(int)_activePanelIndex].SetActive(false);
        }

        private enum PanelIndex
        {
            PlayerList = 0,
            PropsList,
            ToolList
        }
    }
}