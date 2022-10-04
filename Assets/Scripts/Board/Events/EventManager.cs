using System;
using System.Collections.Generic;
using System.Linq;
using Board.Shapes;
using Board.Tools;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Users;
using DeviceType = Utils.DeviceType;
using Event = Utils.Event;

namespace Board.Events
{
    public class EventManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Tools.Tools tools;
        [SerializeField] private PhotonView view;

        [SerializeField] private GameObject vrPrefab;
        [SerializeField] private GameObject arPrefab;
        [SerializeField] private GameObject postItPrefab;
        [SerializeField] private GameObject onlinePingPrefab;
        [SerializeField] private GameObject board;
        
        private List<PlayerEntity> _others;

        #region UNITY

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

            PlayerEvents.SetupPrefabs(postItPrefab, onlinePingPrefab, board);
        }

        #endregion

        #region PHOTON_EVENTS

        private void OnEvent(EventData photonEvent)
        {
            var code = (Event.EventCode)photonEvent.Code;

            switch (photonEvent.Code)
            {
                case < 10:
                    OnRoomEvent(code, photonEvent.CustomData);
                    break;

                case < 20:
                    OnPlayerEvent(code, photonEvent.CustomData, photonEvent);
                    break;

                case < 30:
                    OnToolEvent(code, photonEvent.CustomData);
                    break;

                case < 60:
                    OnObjectEvent(code, photonEvent.CustomData);
                    break;

                case < 70:
                    OnChatEvent(code, photonEvent.CustomData);
                    break;

                case >= 100 and < 200:
                    OnErrorEvent(code, photonEvent.CustomData);
                    break;

                case >= 200:
                    Debug.Log($"Photon Event: {code}");
                    break;

                default:
                    throw new ArgumentException($"Invalid event code: {photonEvent.Code}");
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);

            var raiseEventOptions = new RaiseEventOptions { TargetActors = new[] { newPlayer.ActorNumber } };

            PhotonNetwork.RaiseEvent((byte)Event.EventCode.SendNewPlayerIn, transform.position, raiseEventOptions,
                SendOptions.SendReliable);

            if (!PhotonNetwork.IsMasterClient) return;

            var content = board.GetComponent<Board>().texture.EncodeToPNG();

            PhotonNetwork.RaiseEvent((byte)Event.EventCode.Texture, content, raiseEventOptions,
                SendOptions.SendReliable);

            foreach (var data in from shape in Shape.Shapes
                     let transform1 = shape.Value.transform
                     select new object[] { transform1.position, transform1.rotation, shape.Key })
                PhotonNetwork.RaiseEvent((byte)Event.EventCode.SendNewObject, data, raiseEventOptions,
                    SendOptions.SendReliable);

            foreach (var data in from ping in PlayerEvents.Pings
                     select ping.transform.localPosition
                     into pos
                     let scale = board.transform.localScale.x
                     select new Vector2(pos.x * scale, pos.z * scale))
                PhotonNetwork.RaiseEvent((byte)Event.EventCode.SendNewPing, data, raiseEventOptions,
                    SendOptions.SendReliable);

            foreach (var data in from postIt in PlayerEvents.PostIts
                     let pos = postIt.transform.localPosition
                     let text = postIt.GetComponentInChildren<TMP_Text>().text
                     let scale = board.transform.localScale.x
                     select new object[] { new Vector2(pos.x * scale, pos.z * scale) })
                PhotonNetwork.RaiseEvent((byte)Event.EventCode.SendNewPostIt, data, raiseEventOptions,
                    SendOptions.SendReliable);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);

            Debug.Log(otherPlayer.NickName + "left the room");

            var playerEntity = _others.Find(x => x.photonId == otherPlayer.ActorNumber);
            _others.Remove(playerEntity);
            Destroy(playerEntity.gameObject);
        }

        public override void OnJoinedRoom()
        {
            PhotonNetwork.NickName =
                $"{((DeviceType)PhotonNetwork.LocalPlayer.CustomProperties["Device"] == DeviceType.VR ? "VR" : "AR")}" +
                $" {PhotonNetwork.LocalPlayer.NickName}";

            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte)Event.EventCode.SendNewPlayerIn, transform.position, raiseEventOptions,
                SendOptions.SendReliable);
            
            view.ViewID = 0;
            view.ViewID = PhotonNetwork.LocalPlayer.ActorNumber;
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);

            PlayerEvents.Clear();

            Debug.LogError($"Disconnected from server: {cause}");
        }

        #endregion

        #region ROOM_EVENTS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventCode"></param>
        /// <param name="data"> Unused </param>
        /// <exception cref="ArgumentException"></exception>
        private static void OnRoomEvent(Event.EventCode eventCode, object data)
        {
            switch (eventCode)
            {
                default:
                    throw new ArgumentException("Invalid event code");
            }
        }

        #endregion

        #region PLAYER_EVENTS

        private void OnPlayerEvent(Event.EventCode eventCode, object data, EventData photonEvent)
        {
            switch (eventCode)
            {
                case Event.EventCode.SendNewPostIt:
                    PlayerEvents.ReceiveNewPostIt(data as object[]);
                    break;

                case Event.EventCode.SendNewPosition:
                    _others.Find(p => p.photonId == photonEvent.Sender)
                        .UpdateTransform((Vector3)data);
                    break;

                case Event.EventCode.SendNewPlayerIn:
                    var newPlayer = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender);
                    var playerEntity = AddPlayer(newPlayer);

                    playerEntity.UpdateTransform((Vector3)data);
                    _others.Add(playerEntity);
                    break;

                case Event.EventCode.SendNewPing:
                    PlayerEvents.ReceivePing((Vector2)data);
                    break;

                default:
                    throw new ArgumentException("Unknown event code");
            }
        }

        private PlayerEntity AddPlayer(Player newPlayer)
        {
            print(newPlayer.NickName + " joined the room.");

            var device = (DeviceType)newPlayer.CustomProperties.GetValueOrDefault("Device");
            var entity = Instantiate(device == DeviceType.VR ? vrPrefab : arPrefab, new Vector3(0, 0, 0),
                Quaternion.identity);
            var playerEntity = entity.AddComponent<PlayerEntity>();
            var playerView = entity.GetComponent<PhotonView>();
            
            playerEntity.SetValues(device, newPlayer.ActorNumber, newPlayer.NickName);
            playerView.ViewID = 0;
            playerView.ViewID = newPlayer.ActorNumber;

            return playerEntity;
        }

        #endregion

        #region TOOL_EVENTS

        private void OnToolEvent(Event.EventCode eventCode, object data)
        {
            switch (eventCode)
            {
                case Event.EventCode.Marker:
                    tools.marker.AddModification(new Modification(data));
                    break;

                case Event.EventCode.Eraser:
                    tools.eraser.AddModification(new Modification(data));
                    break;

                case Event.EventCode.Texture:
                    board.GetComponent<Board>().texture.LoadImage(data as byte[]);
                    break;

                default:
                    throw new ArgumentException("Unknown event code");
            }
        }

        #endregion

        #region OBJECT_EVENTS

        private void OnObjectEvent(Event.EventCode eventCode, object data)
        {
            switch (eventCode)
            {
                case Event.EventCode.SendNewObject:
                    Shape.ReceiveNewObject(data as object[]);
                    break;

                case Event.EventCode.SendDestroy:
                    Shape.ReceiveDestroy((int)data);
                    break;

                case Event.EventCode.SendTransform:
                    Shape.ReceiveTransform(data as object[]);
                    break;

                case Event.EventCode.SendOwnership:
                    Shape.ReceiveOwnership(data as object[]);
                    break;

                default:
                    throw new ArgumentException("Invalid event code");
            }
        }

        #endregion

        #region CHAT_EVENTS

        private void OnChatEvent(Event.EventCode eventCode, object data)
        {
            switch (eventCode)
            {
                default:
                    throw new ArgumentException("Invalid event code");
            }
        }

        #endregion

        #region ERROR_EVENTS

        private void OnErrorEvent(Event.EventCode eventCode, object data)
        {
            switch (eventCode)
            {
                default:
                    throw new ArgumentException("Invalid event code");
            }
        }

        #endregion
    }
}