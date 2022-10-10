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
using Utils;
using DeviceType = Utils.DeviceType;
using EventCode = Utils.EventCode;

namespace Board.Events
{
    /// <inheritdoc />
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

        /// <inheritdoc />
        public override void OnEnable()
        {
            base.OnEnable();

            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        }

        /// <inheritdoc />
        public override void OnDisable()
        {
            base.OnDisable();

            PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        }

        private void Awake()
        {
            CustomShape.Init();
        }

        private void Start()
        {
            _others = new List<PlayerEntity>();

            PlayerEvents.SetupPrefabs(postItPrefab, onlinePingPrefab, board);
        }

        #endregion

        #region PHOTON_EVENTS

        private void OnEvent(EventData photonEvent)
        {
            var code = (EventCode)photonEvent.Code;

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

        /// <inheritdoc />
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);

            var raiseEventOptions = new RaiseEventOptions { TargetActors = new[] { newPlayer.ActorNumber } };

            PhotonNetwork.RaiseEvent((byte)EventCode.SendNewPlayerIn, transform.position, raiseEventOptions,
                SendOptions.SendReliable);

#if DEBUG
            DebugPanel.Instance.AddPlayerSent();
#endif

            if (!PhotonNetwork.IsMasterClient) return;

            var content = board.GetComponent<Board>().texture.EncodeToPNG();

            PhotonNetwork.RaiseEvent((byte)EventCode.Texture, content, raiseEventOptions,
                SendOptions.SendReliable);

#if DEBUG
            DebugPanel.Instance.AddBoardSent();
#endif

            foreach (var data in from shape in Shape.Shapes
                     let transform1 = shape.Value.transform
                     select new object[] { transform1.position, transform1.rotation, shape.Key })
            {
                PhotonNetwork.RaiseEvent((byte)EventCode.SendNewObject, data, raiseEventOptions,
                    SendOptions.SendReliable);

#if DEBUG
                DebugPanel.Instance.AddObjectSent();
#endif
            }

            foreach (var data in from ping in PlayerEvents.Pings
                     select ping.transform.localPosition
                     into pos
                     let scale = board.transform.localScale.x
                     select new Vector2(pos.x * scale, pos.z * scale))
            {
                PhotonNetwork.RaiseEvent((byte)EventCode.SendNewPing, data, raiseEventOptions,
                    SendOptions.SendReliable);

#if DEBUG
                DebugPanel.Instance.AddBoardSent();
#endif
            }

            foreach (var data in from postIt in PlayerEvents.PostIts
                     let pos = postIt.transform.localPosition
                     let text = postIt.GetComponentInChildren<TMP_Text>().text
                     let scale = board.transform.localScale.x
                     select new object[] { new Vector2(pos.x * scale, pos.z * scale), text })
            {
                PhotonNetwork.RaiseEvent((byte)EventCode.SendNewPostIt, data, raiseEventOptions,
                    SendOptions.SendReliable);

#if DEBUG
                DebugPanel.Instance.AddPlayerSent();
#endif
            }
        }

        /// <inheritdoc />
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);

            Debug.Log(otherPlayer.NickName + "left the room");

            var playerEntity = _others.Find(x => x.photonId == otherPlayer.ActorNumber);
            _others.Remove(playerEntity);
            Destroy(playerEntity.gameObject);

#if DEBUG
            var deviceType = otherPlayer.CustomProperties.ContainsKey("Device")
                ? (DeviceType)otherPlayer.CustomProperties["Device"]
                : DeviceType.VR;

            DebugPanel.Instance.RemovePlayer(deviceType);
#endif
        }

        /// <inheritdoc />
        public override void OnJoinedRoom()
        {
            PhotonNetwork.NickName =
                $"{((DeviceType)PhotonNetwork.LocalPlayer.CustomProperties["Device"] == DeviceType.VR ? "VR" : "AR")}" +
                $" {PhotonNetwork.LocalPlayer.NickName}";

            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte)EventCode.SendNewPlayerIn, transform.position, raiseEventOptions,
                SendOptions.SendReliable);

#if DEBUG
            DebugPanel.Instance.AddPlayerSent();
            DebugPanel.Instance.SetConnected(true);
#endif

            view.ViewID = 0;
            view.ViewID = PhotonNetwork.LocalPlayer.ActorNumber;
        }

        /// <inheritdoc />
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
        private static void OnRoomEvent(EventCode eventCode, object data)
        {
            switch (eventCode)
            {
                default:
                    throw new ArgumentException("Invalid event code");
            }
        }

        #endregion

        #region PLAYER_EVENTS

        private void OnPlayerEvent(EventCode eventCode, object data, EventData photonEvent)
        {
            switch (eventCode)
            {
                case EventCode.SendNewPostIt:
                    PlayerEvents.ReceiveNewPostIt(data as object[]);
                    break;

                case EventCode.SendNewPosition:
                    _others.Find(p => p.photonId == photonEvent.Sender)
                        .UpdateTransform((Vector3)data);
                    break;

                case EventCode.SendNewPlayerIn:
                    var newPlayer = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender);
                    var playerEntity = AddPlayer(newPlayer);

                    playerEntity.UpdateTransform((Vector3)data);
                    _others.Add(playerEntity);
                    break;

                case EventCode.SendNewPing:
                    PlayerEvents.ReceivePing((Vector2)data);
                    break;

                default:
                    throw new ArgumentException("Unknown event code");
            }

#if DEBUG
            DebugPanel.Instance.AddPlayerReceived();
#endif
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

#if DEBUG
            DebugPanel.Instance.AddPlayer(device);
#endif

            return playerEntity;
        }

        #endregion

        #region TOOL_EVENTS

        private void OnToolEvent(EventCode eventCode, object data)
        {
            switch (eventCode)
            {
                case EventCode.Marker:
                    tools.marker.AddModification(new Modification(data));
                    break;

                case EventCode.Eraser:
                    tools.eraser.AddModification(new Modification(data));
                    break;

                case EventCode.Texture:
                    Board.Instance.texture.LoadImage(data as byte[]);
                    break;

                default:
                    throw new ArgumentException("Unknown event code");
            }

#if DEBUG
            DebugPanel.Instance.AddBoardReceived();
#endif
        }

        #endregion

        #region OBJECT_EVENTS

        private static void OnObjectEvent(EventCode eventCode, object data)
        {
            switch (eventCode)
            {
                case EventCode.SendNewObject:
                    Shape.ReceiveNewObject(data as object[]);
                    break;

                case EventCode.SendDestroy:
                    Shape.ReceiveDestroy((int)data);
                    break;

                case EventCode.SendTransform:
                    Shape.ReceiveTransform(data as object[]);
                    break;

                case EventCode.SendOwnership:
                    Shape.ReceiveOwnership(data as object[]);
                    break;

                default:
                    throw new ArgumentException("Invalid event code");
            }

#if DEBUG
            DebugPanel.Instance.AddObjectReceived();
#endif
        }

        #endregion

        #region CHAT_EVENTS

        private static void OnChatEvent(EventCode eventCode, object data)
        {
            switch (eventCode)
            {
                default:
                    throw new ArgumentException("Invalid event code");
            }
        }

        #endregion

        #region ERROR_EVENTS

        private void OnErrorEvent(EventCode eventCode, object data)
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