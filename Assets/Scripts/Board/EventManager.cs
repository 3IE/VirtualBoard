using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [SerializeField]
    private Tools tools;

    private void OnEnable() => PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    private void OnDisable() => PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;

    public void OnEvent(EventData photonEvent)
    {
        object[] data = (object[]) photonEvent.CustomData;

        switch((Event.EventCode) photonEvent.Code)
        {
            case Event.EventCode.Marker:
                tools.marker.AddModification((Modification) data[0]);
                break;

            case Event.EventCode.Eraser:
                tools.eraser.AddModification((Modification) data[0]);
                break;
        }
    }
}
