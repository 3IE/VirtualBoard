using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class EventManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Tools tools;
    [SerializeField]
    private GameObject VRPrefab;
    [SerializeField]
    private GameObject ARPrefab;
    [SerializeField]
    private GameObject postItPrefab;
    [SerializeField]
    private GameObject VRAvatarPrefab;
    [SerializeField]
    private GameObject ARAvatarPrefab;
    [SerializeField]
    private GameObject onlinePingPrefab;
    [SerializeField]
    private GameObject boardPrefab;

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
        if ((Event.EventCode) photonEvent.Code != Event.EventCode.SendNewPosition)
            print($"New code received: {(Event.EventCode) photonEvent.Code}");
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
                others.Add(playerEntity);
                playerEntity.UpdateTransform((Vector3)data[0]);
                others.Add(playerEntity);
                
                break;
            case Event.EventCode.SendNewPostIt:
                var postItPos = (Vector2) data[0];
                var text = (string) data[1];

                var boardPos_Postit = boardPrefab.transform.position;
                OnlinePostIt(
                    new Vector3(boardPos_Postit.x + postItPos.x, boardPos_Postit.y + postItPos.y, boardPos_Postit.z)
                    , text, Color.cyan); // On verra plus tard pour que la couleur varie en fc du joueur
                break;
            case  Event.EventCode.SendNewPing:
                var pingPos = (Vector2) data[0];
                OnlinePing(pingPos);
                break;
        }
    }

    private PlayerEntity AddPlayer(Player newPlayer)
    {
        print(newPlayer.NickName + " joined the room.");

        DeviceType device = (DeviceType)newPlayer.CustomProperties.GetValueOrDefault("Device");

        GameObject entity = Instantiate(device == DeviceType.VR ? VRPrefab : ARPrefab, new Vector3(0, 1.05f, 0), Quaternion.identity);
        PlayerEntity playerEntity = entity.AddComponent<PlayerEntity>();
        playerEntity.SetValues(device, newPlayer.ActorNumber, newPlayer.NickName);

        return playerEntity;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

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
    
    private void OnlinePing(Vector2 position)
    {
        var ping = Instantiate(onlinePingPrefab, new Vector3(0, -10, 0), boardPrefab.transform.rotation, boardPrefab.transform);
        ping.transform.localPosition = new Vector3(position.x, position.y, 0);
        //TODO: add PingSearcher
        //_currentPing = ping.transform.position;
        //_pingSearcher.AssignedPing = ping;
        //_pingSearcher.gameObject.SetActive(true);
    }
    private void OnlinePostIt(Vector3 position, string text, Color color)
    {
        var postIt = Instantiate(postItPrefab,  position, boardPrefab.transform.rotation, boardPrefab.transform);
        postIt.GetComponentInChildren<TMP_Text>().text = text;
        postIt.GetComponentInChildren<Renderer>().material.color = color;
        //print($"Post-it: {text}, at {position}");
    }
}
