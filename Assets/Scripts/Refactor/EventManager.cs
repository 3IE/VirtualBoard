using System;
using System.Collections.Generic;
using System.Linq;
using Board.Events;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Shapes;
using TMPro;
using UnityEngine;
using Users;
using Utils;
using DeviceType = Utils.DeviceType;
using EventCode = Utils.EventCode;

namespace Refactor
{
    /// <inheritdoc />
    public class EventManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private GameObject postItPrefab;
        [SerializeField] private GameObject onlinePingPrefab;
        [SerializeField] private GameObject board;

        internal static Dictionary<int, PlayerManagerV2> Players;

        private BoardSetter _boardSetter;

        #region ROOM_EVENTS

        /// <summary>
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

        #region TOOL_EVENTS

        private void OnToolEvent(EventCode eventCode, object data, EventData photonEvent)
        {
            switch (eventCode)
            {
                case EventCode.Texture:
                    _boardSetter.SetTexture(data as object[]);
                    break;

                case EventCode.MarkerGrab:
                    Players[photonEvent.Sender].ReceiveMarkerGrab(data);
                    break;

                case EventCode.MarkerPosition:
                    Players[photonEvent.Sender].ReceiveMarkerPosition(data);
                    break;

                case EventCode.MarkerColor:
                    Players[photonEvent.Sender].ReceiveMarkerColor(data);
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
                    Shape.ReceiveDestroy((int) data);
                    break;

                case EventCode.SendTransform:
                    Shape.ReceiveTransform(data as object[]);
                    break;

                case EventCode.SendOwnership:
                    Shape.ReceiveOwnership(data as object[]);
                    break;

                case EventCode.SendCounter:
                    Shape.ReceiveCounter((int) data);
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

        #region PLAYER_EVENTS

        private void OnPlayerEvent(EventCode eventCode, object data, EventData photonEvent)
        {
            switch (eventCode)
            {
                case EventCode.SendNewPostIt:
                    PlayerEvents.ReceiveNewPostIt(data as object[]);
                    break;

                case EventCode.SendNewPosition:
                    try
                    {
                        Players[photonEvent.Sender].entity.UpdateTransforms(data as object[]);
                    } catch (Exception)
                    {
                        Debug.LogWarning("Player not found, probably not instantiated yet");
                    }

                    break;

                case EventCode.SendNewPlayerIn:
                    break;

                case EventCode.SendNewPing:
                    PlayerEvents.ReceivePing((Vector2) data);
                    break;

                default:
                    throw new ArgumentException("Unknown event code");
            }

            #if DEBUG
            DebugPanel.Instance.AddPlayerReceived();
            #endif
        }

        #endregion

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

            _boardSetter = board.GetComponent<BoardSetter>();

            Players = new Dictionary<int, PlayerManagerV2>();
        }

        private void Start()
        {
            PlayerEvents.SetupPrefabs(postItPrefab, onlinePingPrefab, board);
        }

        #endregion

        #region PHOTON_EVENTS

        private void OnEvent(EventData photonEvent)
        {
            var code = (EventCode) photonEvent.Code;

            switch (photonEvent.Code)
            {
                case < 10:
                    OnRoomEvent(code, photonEvent.CustomData);
                    break;

                case < 20:
                    OnPlayerEvent(code, photonEvent.CustomData, photonEvent);
                    break;

                case < 30:
                    OnToolEvent(code, photonEvent.CustomData, photonEvent);
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
                    return;

                default:
                    throw new ArgumentException($"Invalid event code: {photonEvent.Code}");
            }

            #if DEBUG
            DebugPanel.Instance.AddReceived();
            #endif
        }

        /// <inheritdoc />
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);

            #if DEBUG
            if (newPlayer.CustomProperties.TryGetValue("Device", out object type)
                && type is DeviceType deviceType and not DeviceType.Unknown)
                DebugPanel.Instance.AddPlayer(deviceType);
            #endif

            if (!PhotonNetwork.IsMasterClient) return;

            var raiseEventOptions = new RaiseEventOptions { TargetActors = new[] { newPlayer.ActorNumber } };

            object[] content = _boardSetter.GetTexture();

            if (content is not null)
                PhotonNetwork.RaiseEvent((byte) EventCode.Texture, content, raiseEventOptions,
                                         SendOptions.SendReliable);

            PhotonNetwork.RaiseEvent((byte) EventCode.SendCounter, Shape.Counter, raiseEventOptions,
                                     SendOptions.SendReliable);

            foreach (object[] data in from shape in Shape.Shapes
                                      let shapeTransform = shape.Value.transform
                                      let id = shape.Value.ShapeId
                                      select new object[] { shapeTransform.position, shapeTransform.rotation, id })
            {
                PhotonNetwork.RaiseEvent((byte) EventCode.SendNewObject, data, raiseEventOptions,
                                         SendOptions.SendReliable);

                #if DEBUG
                DebugPanel.Instance.AddObjectSent();
                #endif
            }

            foreach (Vector2 data in from ping in PlayerEvents.Pings
                                     select ping.transform.localPosition
                                     into pos
                                     let scale = board.transform.localScale.x
                                     select new Vector2(pos.x * scale, pos.z * scale))
            {
                PhotonNetwork.RaiseEvent((byte) EventCode.SendNewPing, data, raiseEventOptions,
                                         SendOptions.SendReliable);

                #if DEBUG
                DebugPanel.Instance.AddBoardSent();
                #endif
            }

            foreach (object[] data in from postIt in PlayerEvents.PostIts
                                      let pos = postIt.transform.localPosition
                                      let text = postIt.GetComponentInChildren<TMP_Text>().text
                                      let scale = board.transform.localScale.x
                                      select new object[] { new Vector2(pos.x * scale, pos.z * scale), text })
            {
                PhotonNetwork.RaiseEvent((byte) EventCode.SendNewPostIt, data, raiseEventOptions,
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

            #if DEBUG
            DeviceType deviceType = otherPlayer.CustomProperties.ContainsKey("Device")
                ? (DeviceType) otherPlayer.CustomProperties["Device"]
                : DeviceType.VR;

            DebugPanel.Instance.RemovePlayer(deviceType);
            #endif
        }

        /// <inheritdoc />
        public override void OnJoinedRoom()
        {
            #if DEBUG
            DebugPanel.Instance.SetConnected(true);
            #endif

            VrPlayerManager.Connected = true;
        }

        /// <inheritdoc />
        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);

            PlayerEvents.Clear();

            Debug.LogError($"Disconnected from server: {cause}");
        }

        #endregion
    }
}