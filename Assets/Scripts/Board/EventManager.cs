using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class EventManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Tools tools;
    [SerializeField]
    private GameObject VRPrefab;
    [SerializeField]
    private GameObject ARPrefab;

    private List<PlayerEntity> others;

    public override void OnEnable()
   {
        base.OnEnable();

        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public void Start()
    {
        others = new List<PlayerEntity>();
    }

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

            case Event.EventCode.SendNewPosition:
                int x = photonEvent.Sender;
                others.Find(p => p.photonId == x)
                    .UpdateTransform((Vector3) data[0]);
                break;

            case Event.EventCode.SendNewPlayerIn:
                Player newPlayer = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender);
                
                PlayerEntity playerEntity = AddPlayer(newPlayer);
                playerEntity.UpdateTransform((Vector3)data[0]);
                others.Add(playerEntity);
                
                break;
        }
    }

    private PlayerEntity AddPlayer(Player newPlayer)
    {
        PrintVar.Print(newPlayer.NickName + " joined the room.");

        DeviceType device = (DeviceType)newPlayer.CustomProperties.GetValueOrDefault("Device");

        GameObject entity = Instantiate(device == DeviceType.VR ? VRPrefab : ARPrefab, new Vector3(0, 1.05f, 0), Quaternion.identity);
        PlayerEntity playerEntity = entity.AddComponent<PlayerEntity>();
        playerEntity.SetValues(device, newPlayer.ActorNumber, newPlayer.NickName);

        return playerEntity;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        others.Add(AddPlayer(newPlayer));

        object[] content = { transform.position };

        var raiseEventOptions = new RaiseEventOptions { TargetActors = new int[] { newPlayer.ActorNumber } };

        PhotonNetwork.RaiseEvent((byte) Event.EventCode.SendNewPlayerIn, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        PrintVar.Print(otherPlayer.NickName + "left the room");

        PlayerEntity playerEntity = others.Find(x => x.photonId == otherPlayer.ActorNumber);
        others.Remove(playerEntity);
        Destroy(playerEntity.gameObject);
    }
}
