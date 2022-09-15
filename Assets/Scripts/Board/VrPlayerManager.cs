using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class VrPlayerManager : MonoBehaviour
{
    // a attacher au XR Origin/Camera Offset
    private Transform VRCamTransform;
    [SerializeField] private Transform BoardTransform;
    [SerializeField] private float _refreshRate = 0.2f;

    private void OnEnable()
    {
        VRCamTransform = GetComponentInChildren<Camera>().transform;
        InvokeRepeating(nameof(SendNewPositionEvent), _refreshRate, _refreshRate);
    }
    private void OnDisable() => CancelInvoke();

    private void SendNewPositionEvent()
    {
        object[] content = { VRCamTransform.position - BoardTransform.position };
        
        var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

        PhotonNetwork.RaiseEvent((byte) Event.EventCode.SendNewPosition, content, raiseEventOptions, SendOptions.SendReliable);
    }
    
    //! TO DELETE LATER
    public void HahaPinged()
    {
        object[] content = { new Vector2(1, 1)};
        var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent((byte) Event.EventCode.SendNewPing, content, raiseEventOptions, SendOptions.SendReliable);
    }
}
