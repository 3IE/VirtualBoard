using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Voice.PUN;
using UnityEngine;
using Utils;
using DeviceType = Utils.DeviceType;

namespace Refactor
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;

        [SerializeField] private Transform mainCamera;
        [SerializeField] private Transform leftInteractor;
        [SerializeField] private Transform rightInteractor;

        [SerializeField] private DeviceType deviceType = DeviceType.VR;

        private void Awake()
        {
            PhotonNetwork.SetPlayerCustomProperties(new Hashtable
                                                        { { "Device", deviceType } });

            Debug.Log("Connecting");
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Joining room");
            PhotonNetwork.JoinRandomOrCreateRoom();
        }

        public override void OnCreatedRoom()
        {
            Debug.Log("Created room");

            #if DEBUG
            DebugPanel.Instance.SetConnected(true);
            #endif

            base.OnCreatedRoom();
            InstantiatePlayer();
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined room");

            #if DEBUG
            DebugPanel.Instance.SetConnected(true);
            #endif

            base.OnJoinedRoom();
            InstantiatePlayer();
        }

        private void InstantiatePlayer()
        {
            if (playerPrefab is null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",
                               this);
            }
            else if (PlayerManager.LocalPlayerInstance is null)
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}\nPrefab name: {1}",
                                SceneManagerHelper.ActiveSceneName, this.playerPrefab.name);

                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                GameObject player =
                    PhotonNetwork.Instantiate(this.playerPrefab.name, mainCamera.position, mainCamera.rotation);
                player.transform.SetParent(mainCamera);

                var entity = player.GetComponent<PlayerEntity>();
                entity.SetDevice(DeviceType.VR);
                entity.ReplaceHandsTransforms(leftInteractor, rightInteractor);
                
                #if DEBUG
                DebugPanel.Instance.SetConnected(true);
                DebugPanel.Instance.AddPlayer(DeviceType.VR);
                #endif
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
    }
}