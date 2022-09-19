using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class Shape : MonoBehaviour
{
    [SerializeField]
    protected bool selectedLeft = false;
    [SerializeField]
    protected bool selectedRight = false;

    protected bool locked = false;
    protected bool isOwner = false;

    protected Transform transformLeft;
    protected Transform transformRight;

    protected float initialDistance;
    protected Vector3 initialScale;

    private bool moving = false;
    private bool resizing = false;

    private Material mat;

    private List<IXRSelectInteractor> interactors;

    #region Unity

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        interactors = new List<IXRSelectInteractor>(2);

        Create();
    }

    private void Update()
    {
        if (moving)
            Move();
        if (resizing)
            Resize();
    }

    #endregion

    #region Material

    public void Create()
    {
        mat.SetFloat("_Create", 1);
    }

    public void Modify()
    {
        mat.SetFloat("_Modify", 1);
    }

    public void Destroy()
    {
        mat.SetFloat("_Destroy", 1);
    }

    public void StopAction()
    {
        mat.SetFloat("_Create", 0);
        mat.SetFloat("_Modify", 0);
        mat.SetFloat("_Destroy", 0);
    }

    public void CreateAction()
    {
        StopAction();
        locked = true;
    }

    #endregion

    #region Selection

    public void OnSelect(SelectEnterEventArgs args)
    {
        if (!isOwner && locked)
            return;

        isOwner = true;
        locked = true;

        SendOwnership();

        interactors.Add(args.interactorObject);
    }

    public void OnSelectLeft()
    {
        { 
            if (nbSelected == 2)
            {
                resizing = true;
                initialDistance = Vector3.Distance(transformLeft.position, transformRight.position);
                initialScale = transform.localScale;
            }

            Modify();
        }
    }

    public void OnDeselect()
    {
    }

    #endregion

    #region Networking

    public void UpdateTransform()
    {
        throw new System.NotImplementedException();
    }

    public void SendTransform()
    {
        if (isOwner && locked)
            throw new System.NotImplementedException();
    }

    public void SendDestroy()
    {
        if (isOwner && locked)
            throw new System.NotImplementedException();
    }

    public void ReceiveDestroy()
    {
        Destroy();
    }

    public void SendOwnership()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateOwnership()
    {
        throw new System.NotImplementedException();
    }

    #endregion

    #region Abstract

    /// <summary>
    /// Moves the object along the ray of the controller selecting it
    /// </summary>
    protected abstract void Move();

    /// <summary>
    /// Resizes the object according to the distance between the two controllers
    /// </summary>
    protected abstract void Resize();

    #endregion
}
