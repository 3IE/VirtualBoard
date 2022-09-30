using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class VRMenu : MonoBehaviour
{
    // List of MenuPanels with their respective buttons
    [SerializeField] private List<GameObject> panels;
    [SerializeField] private List<GameObject> panelsButtons;
    private PanelIndex activePanelIndex = PanelIndex.PlayerList;

    private Rigidbody _rigidbody;
    private CanvasGroup _canvasGroup;
    [SerializeField] private float throwThreshold;
    [SerializeField] private float timeToFade;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ThrowAway() => StartCoroutine(ThrowAwayRoutine());
    private IEnumerator ThrowAwayRoutine()
    {
        PrintVar.print(1, "Destroying");
        if (_rigidbody.velocity.magnitude < throwThreshold) yield break;
        float i = 0.0f;
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
        _rigidbody.velocity = Vector3.zero;
        _canvasGroup.alpha = 1;
    }

    public void SwitchPanel(int index) => SwitchPanel((PanelIndex) index);
    public void SwitchPanel(PanelIndex index)
    {
        panels[(int)activePanelIndex].SetActive(false);
        panelsButtons[(int)activePanelIndex].SetActive(true);
        activePanelIndex = index;
        panels[(int)activePanelIndex].SetActive(true);
        panelsButtons[(int)activePanelIndex].SetActive(false);
    }
    public enum PanelIndex
    {
        PlayerList = 0,
        PropsList,
        ToolList
    }

}
