using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using DeviceType = Utils.DeviceType;

namespace Launcher
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private TMPro.TMP_InputField usernameInput;

        private const string GameVersion = "1";

        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        [SerializeField]
        private byte maxPlayersPerRoom = 16;

        [SerializeField]
        private GameObject logView;

        [SerializeField]
        private GameObject connectButton;

        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = false;

            PhotonNetwork.SetPlayerCustomProperties(new Hashtable { { "Device", DeviceType.VR } });

#if UNITY_EDITOR
            //logView.SetActive(true);
#endif
        }

        private void Start()
        {
            Connect();
        }

        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect()
        {
            if (usernameInput.text.Length == 0)
                return;

            PhotonNetwork.NickName = usernameInput.text;

            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinLobby();
            }
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = GameVersion;
            }
        }

        public override void OnConnectedToMaster()
        {
#if UNITY_EDITOR
            PrintVar.PrintDebug("PUN Launcher: OnConnectedToMaster() was called by PUN");
#endif
        
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();

#if UNITY_EDITOR
            PrintVar.PrintDebug("PUN Launcher: OnJoinedLobby() was called by PUN");
#endif

            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
#if UNITY_EDITOR
            PrintVar.PrintDebug("PUN Launcher: OnDisconnected() was called by PUN with reason " + cause);
#endif

            connectButton.SetActive(true);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
#if UNITY_EDITOR
            PrintVar.PrintDebug("PUN Launcher: OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
#endif

            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
        }

        public override void OnJoinedRoom()
        {
#if UNITY_EDITOR
            PrintVar.PrintDebug("PUN Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
#endif

            SceneManager.LoadScene(1);
        }

        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();

#if UNITY_EDITOR
            PrintVar.PrintDebug("PUN Launcher: OnCreatedRoom() called by PUN. Now this client is in a room.");
#endif

            SceneManager.LoadScene(1);
        }

        public void Cancel()
        {
#if UNITY_EDITOR
            PrintVar.PrintDebug("Launcher: Cancel() called by Launcher.");
#endif

            connectButton.SetActive(false);
            PhotonNetwork.Disconnect();
        }
    }
}
