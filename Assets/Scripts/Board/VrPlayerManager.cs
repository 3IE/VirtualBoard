using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Event = Utils.Event;

public class VrPlayerManager : MonoBehaviour
{
    // a attacher au XR Origin/Camera Offset
    private Transform VRCamTransform;
    [SerializeField] 
    private Transform BoardTransform;
    [SerializeField] 
    private float _refreshRate = 0.2f;
    [SerializeField]
    private GameObject localPingPrefab;

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
    private void Ping(Vector3 position)
    { // pooling des ping, 2 prefab de ping (un pour l'utilisateur et un pour les autres) 
        // ping physique
        var ping = Instantiate(localPingPrefab,  position, BoardTransform.rotation, BoardTransform);
        // ping sur le reseaux
        var localPos = ping.transform.localPosition;
        SendNewPingEvent(new Vector2(localPos.x, localPos.y));
    }
    private void SendNewPingEvent(Vector2 position) {
        object[] content = { position };
        var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent((byte) Event.EventCode.SendNewPing, content, raiseEventOptions, SendOptions.SendReliable);
    }
    
    //! TO DELETE LATER
    public void HahaPinged()
    {
        object[] content = { new Vector2(1, 1)};
        var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent((byte) Event.EventCode.SendNewPing, content, raiseEventOptions, SendOptions.SendReliable);
    }
}
