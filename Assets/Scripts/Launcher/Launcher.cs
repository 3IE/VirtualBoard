using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TMPro.TMP_InputField usernameInput;

    private string _gameVersion = "1";

    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 16;

    [SerializeField]
    private GameObject LogView;

    [SerializeField]
    private GameObject ConnectButton;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = false;

        Hashtable hash = new Hashtable();
        hash.Add("Device", DeviceType.VR);

        PhotonNetwork.SetPlayerCustomProperties(hash);

        #if UNITY_EDITOR
        LogView.SetActive(true);
        #endif
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
            PhotonNetwork.GameVersion = _gameVersion;
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

        ConnectButton.SetActive(true);
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

        ConnectButton.SetActive(false);
        PhotonNetwork.Disconnect();
    }
}
