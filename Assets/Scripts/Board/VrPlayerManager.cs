using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class VrPlayerManager : MonoBehaviour
{
    // a attacher au XR Origin/Camera Offset
    private Transform VRCamTransform;
    
    [SerializeField] private float _refreshRate = 1f;

    private void OnEnable()
    {
        VRCamTransform = GetComponentInChildren<Camera>().transform;
        InvokeRepeating(nameof(SendNewPositionEvent), _refreshRate, _refreshRate);
    }
    private void OnDisable() => CancelInvoke();

    private void SendNewPositionEvent()
    {
        object[] content = { VRCamTransform.position };
        
        var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

        PhotonNetwork.RaiseEvent((byte) Event.EventCode.SendNewPosition, content, raiseEventOptions, SendOptions.SendReliable);
    }
}
