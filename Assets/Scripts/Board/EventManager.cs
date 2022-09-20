using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Utils;
using DeviceType = Utils.DeviceType;
using Event = Utils.Event;

namespace Board
{
    public class EventManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Tools tools;
        [SerializeField] private GameObject vrPrefab;
        [SerializeField] private GameObject arPrefab;
        [SerializeField] private GameObject postItPrefab;
        [SerializeField] private GameObject vrAvatarPrefab;
        [SerializeField] private GameObject arAvatarPrefab;
        [SerializeField] private GameObject onlinePingPrefab;
        [SerializeField] private GameObject boardPrefab;

        private List<PlayerEntity> _others;

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
            _others = new List<PlayerEntity>();
        }

        public void OnEvent(EventData photonEvent)
        {
            var data = (object[])photonEvent.CustomData;
            if ((Event.EventCode)photonEvent.Code != Event.EventCode.SendNewPosition)
                print($"New code received: {(Event.EventCode)photonEvent.Code}");

            switch ((Event.EventCode)photonEvent.Code)
            {
                case Event.EventCode.Marker:
                    tools.marker.AddModification((Modification)data[0]);
                    break;

                case Event.EventCode.Eraser:
                    tools.eraser.AddModification((Modification)data[0]);
                    break;

                case Event.EventCode.SendNewPosition:
                    var x = photonEvent.Sender;
                    _others.Find(p => p.photonId == x)
                        .UpdateTransform((Vector3)data[0]);
                    break;

                case Event.EventCode.SendNewPlayerIn:
                    var newPlayer = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender);

                    var playerEntity = AddPlayer(newPlayer);
                    _others.Add(playerEntity);
                    playerEntity.UpdateTransform((Vector3)data[0]);
                    _others.Add(playerEntity);

                    break;
                case Event.EventCode.SendNewPostIt:
                    var postItPos = (Vector2)data[0];
                    var text = (string)data[1];

                    var boardPosPostit = boardPrefab.transform.position;
                    OnlinePostIt(
                        new Vector3(boardPosPostit.x + postItPos.x, boardPosPostit.y + postItPos.y, boardPosPostit.z)
                        , text, Color.cyan); // On verra plus tard pour que la couleur varie en fc du joueur
                    break;
                case Event.EventCode.SendNewPing:
                    var pingPos = (Vector2)data[0];
                    OnlinePing(pingPos);
                    break;
            }
        }

        private PlayerEntity AddPlayer(Player newPlayer)
        {
            print(newPlayer.NickName + " joined the room.");

            var device = (DeviceType)newPlayer.CustomProperties.GetValueOrDefault("Device");

            var entity = Instantiate(device == DeviceType.VR ? vrPrefab : arPrefab, new Vector3(0, 1.05f, 0),
                Quaternion.identity);
            var playerEntity = entity.AddComponent<PlayerEntity>();
            playerEntity.SetValues(device, newPlayer.ActorNumber, newPlayer.NickName);

            return playerEntity;
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);

            object[] content = { transform.position };

            var raiseEventOptions = new RaiseEventOptions { TargetActors = new[] { newPlayer.ActorNumber } };

            PhotonNetwork.RaiseEvent((byte)Event.EventCode.SendNewPlayerIn, content, raiseEventOptions,
                SendOptions.SendReliable);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);

            PrintVar.Print(otherPlayer.NickName + "left the room");

            var playerEntity = _others.Find(x => x.photonId == otherPlayer.ActorNumber);
            _others.Remove(playerEntity);
            Destroy(playerEntity.gameObject);
        }

        private void OnlinePing(Vector2 position)
        {
            var ping = Instantiate(onlinePingPrefab, new Vector3(0, -10, 0), boardPrefab.transform.rotation,
                boardPrefab.transform);
            ping.transform.localPosition = new Vector3(position.x, position.y, 0);
            //TODO: add PingSearcher
            //_currentPing = ping.transform.position;
            //_pingSearcher.AssignedPing = ping;
            //_pingSearcher.gameObject.SetActive(true);
        }

        private void OnlinePostIt(Vector3 position, string text, Color color)
        {
            var postIt = Instantiate(postItPrefab, position, boardPrefab.transform.rotation, boardPrefab.transform);
            postIt.GetComponentInChildren<TMP_Text>().text = text;
            postIt.GetComponentInChildren<Renderer>().material.color = color;
            //print($"Post-it: {text}, at {position}");
        }
    }
}