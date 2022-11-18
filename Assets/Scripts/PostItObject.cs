using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PostItObject : XRGrabInteractable, IPunObservable
{
    [SerializeField] private bool  owned;
    [SerializeField] private bool  ownedByMe;
    [SerializeField] private bool  anchored;

    public bool OwnedByMe
    {
        set => owned = ownedByMe = value;
    }
    
    private Transform             camTransform;
    private Vector3               initialScale;
    private float                 initialDistance;
    
    private TMP_InputField        inputField;
    private PhotonTransformView   photonTransformView;
    private Transform             transformComponent;

    protected override void Awake()
    {
        base.Awake();
        selectMode = InteractableSelectMode.Multiple;
        if (Camera.main != null) 
            camTransform           = Camera.main.transform;
        inputField                 = GetComponentInChildren<TMP_InputField>();
        photonTransformView        = GetComponent<PhotonTransformView>();
        transformComponent         = GetComponent<Transform>();
    }

    private void Update()
    {
        if (anchored || owned) return;
        transformComponent.LookAt(camTransform);
    }

    public void OnGrab()
    {
        OwnedByMe = true;
        transformComponent.SetParent(null);
        if (interactorsSelecting.Count == 2)
        {
            initialScale = transformComponent.localScale;
            initialDistance = Vector3.Distance(interactorsSelecting[0].transform.position,
                                                interactorsSelecting[1].transform.position);
        }
    }
    
    // private void OnTriggerEnter(Collider other)
    // {
    //     //TODO: Check if the collider is board or object to stick
    //     if (!Physics.Raycast(transformComponent.position, -transformComponent.forward, out var hit, 1f)) 
    //         return;
    //     if (!hit.collider.CompareTag("Board") && !hit.collider.CompareTag("Board")) 
    //         return;
    //         
    //     cylinderMaterial.color = Color.green;
    // }
    
    public void OnReleasing()
    {
        if (interactorsSelecting.Count == 1)
            return;
        OwnedByMe = false;
        initialDistance = 0;
        if (!Physics.Raycast(transformComponent.position, -transformComponent.forward, out RaycastHit hit, 0.3f)) 
            return;
        Debug.Log("Hit: " + hit.collider.name);
        if (!hit.collider.CompareTag("Board") && !hit.collider.CompareTag("Object")) 
            return;
        anchored = true;
        transformComponent.SetParent(hit.collider.transform);
        //photonTransformView.enabled = false;
        transformComponent.position = hit.point;
        transformComponent.rotation = Quaternion.LookRotation(hit.normal,
            Vector3.Angle(transformComponent.up, Vector3.up) > 30f ?
                transformComponent.up 
                : Vector3.up);
    }
    
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        switch (interactorsSelecting.Count)
        {
            case 1:
                base.ProcessInteractable(updatePhase);
                break;

            case 2 when updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                Resize(interactorsSelecting);
                break;
        }
    }
    
    /// <summary>
    ///     Resizes the object according to the distance between the two controllers
    /// </summary>
    private void Resize(IReadOnlyList<IXRSelectInteractor> Interactors)
    {
        if (Interactors[0].transform.position == Interactors[1].transform.position 
            || initialDistance == 0)
            return;

        transform.localScale =
            initialScale
            / initialDistance
            * Vector3.Distance(Interactors[0].transform.position, Interactors[1].transform.position);
    }
    
    //private void OnTriggerExit(Collider other)
    //{
    //    cylinderMaterial.color = Color.black;
    //}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(owned);
            stream.SendNext(anchored);
            stream.SendNext(inputField.text);
            stream.SendNext(transformComponent.position);
            if (ownedByMe)
            {
                photonTransformView.enabled = true;
                stream.SendNext(transformComponent.rotation);
            }
            else
                photonTransformView.enabled = false;
        }
        else
        {
            owned                       = (bool) stream.ReceiveNext();
            anchored                    = (bool) stream.ReceiveNext();
            inputField.text             = (string) stream.ReceiveNext();
            transformComponent.position = (Vector3) stream.ReceiveNext();
            if (owned)
            {
                photonTransformView.m_SynchronizeRotation = true;
                transformComponent.rotation               = (Quaternion) stream.ReceiveNext();
            }
            else
                photonTransformView.m_SynchronizeRotation = false;
        }
    }
}
