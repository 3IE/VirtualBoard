using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

namespace Board
{
    /// <summary>
    /// Singleton class, holds the current state of the board
    /// </summary>
    public class Board : MonoBehaviourPunCallbacks
    {
        private const string GameVersion = "1";

        public static Board Instance { get; private set; }

        public Texture2D texture;
        public Vector2 textureSize = new(2048, 2048);
        public Tools.Tools tools;

        private void Awake()
        {
            PhotonNetwork.GameVersion = GameVersion;
            PhotonNetwork.SetPlayerCustomProperties(new Hashtable { { "Device", Utils.DeviceType.VR } });
            PhotonNetwork.ConnectUsingSettings();

            var r = GetComponent<Renderer>();

            texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
            tools.baseColor = texture.GetPixel(0, 0);
            r.material.mainTexture = texture;
            
            Instance = this;
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to master");
            PhotonNetwork.JoinRandomOrCreateRoom();
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined room");
        }
    }
}