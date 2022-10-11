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

        /// <summary>
        /// Instance of the class
        /// </summary>
        public static Board Instance { get; private set; }

        /// <summary>
        /// Texture of the board
        /// </summary>
        public Texture2D texture;
        /// <summary>
        /// Size of the texture
        /// </summary>
        public Vector2 textureSize = new(2048, 2048);

        private void Awake()
        {
            PhotonNetwork.GameVersion = GameVersion;
            PhotonNetwork.SetPlayerCustomProperties(new Hashtable { { "Device", Utils.DeviceType.VR } });
            PhotonNetwork.ConnectUsingSettings();

            var r = GetComponent<Renderer>();

            texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
            Tools.Tools.Instance.baseColor = texture.GetPixel(0, 0);
            r.material.mainTexture = texture;
            
            Instance = this;
        }

        /// <inheritdoc />
        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to master");
            PhotonNetwork.JoinRandomOrCreateRoom();
        }

        /// <inheritdoc />
        public override void OnJoinedRoom()
        {
            Debug.Log("Joined room");
        }
    }
}