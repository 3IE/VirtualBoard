using System;
using System.IO;
using System.Text.RegularExpressions;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Board
{
    public class Board : MonoBehaviourPunCallbacks
    {
        private const string GameVersion = "1";
        
        [SerializeField] private InputActionReference captureBoard;

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
            
            captureBoard.action.started += _ => Capture();
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

        private void Capture()
        {
            var now = DateTime.Now;
            var path = $"./board {now:s}.png";
            
            //var fileStream = File.OpenWrite(path);
            //var bytes = texture.EncodeToPNG();
            //
            //fileStream.Write(bytes, 0, bytes.Length);
            //fileStream.Close();
        }
    }
}