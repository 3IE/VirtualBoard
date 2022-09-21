using System;
using System.Collections.Generic;
using Board.Shapes;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Utils;
using DeviceType = Utils.DeviceType;
using Event = Utils.Event;
using Object = UnityEngine.Object;

namespace Board.Events
{
    public class EventManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Tools tools;
        [SerializeField] private GameObject vrPrefab;
        [SerializeField] private GameObject arPrefab;
        [SerializeField] private GameObject postItPrefab;
        [SerializeField] private GameObject onlinePingPrefab;
        [SerializeField] private GameObject boardPrefab;

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
            
            PlayerEvents.SetupPrefabs(postItPrefab, onlinePingPrefab, boardPrefab);
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
                
                default:
                    throw new ArgumentException("Invalid event code");
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);

            var raiseEventOptions = new RaiseEventOptions { TargetActors = new[] { newPlayer.ActorNumber } };

            PhotonNetwork.RaiseEvent((byte)Event.EventCode.SendNewPlayerIn, transform.position, raiseEventOptions,
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

        #endregion

        #region ROOM_EVENTS

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

            var entity = Instantiate(device == DeviceType.VR ? vrPrefab : arPrefab, new Vector3(0, 1.05f, 0),
                Quaternion.identity);
            var playerEntity = entity.AddComponent<PlayerEntity>();
            playerEntity.SetValues(device, newPlayer.ActorNumber, newPlayer.NickName);

            return playerEntity;
        }

        #endregion

        #region TOOL_EVENTS

        private void OnToolEvent(Event.EventCode eventCode, object data)
        {
            switch (eventCode)
            {
                case Event.EventCode.Marker:
                    tools.marker.AddModification((Modification)data);
                    break;

                case Event.EventCode.Eraser:
                    tools.eraser.AddModification((Modification)data);
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