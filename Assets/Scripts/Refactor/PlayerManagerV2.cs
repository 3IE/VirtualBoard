using Photon.Pun;
using UnityEngine;
using DeviceType = Utils.DeviceType;

namespace Refactor
{
    public class PlayerManagerV2 : MonoBehaviour
    {
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        public PlayerEntityV2 entity;

        private MarkerSync _markerSync;
        private GameObject _marker;

        [SerializeField] private DeviceType deviceType;
        [SerializeField] private GameObject markerPrefab;

        private void OnDestroy()
        {
            Destroy(_marker);
        }

        private void Start()
        {
            var photonView = GetComponent<PhotonView>();

            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;

                _markerSync = MarkerSync.LocalInstance;
                _marker     = _markerSync.gameObject;

                entity.SetOwnership();

                EventManager.Players.Add(PhotonNetwork.LocalPlayer.ActorNumber, this);
            }
            else
            {
                deviceType = photonView.Owner.CustomProperties.TryGetValue("Device", out object type)
                    ? (DeviceType) type
                    : DeviceType.Unknown;

                _marker     = Instantiate(markerPrefab, Vector3.zero, Quaternion.identity);
                _markerSync = _marker.GetComponent<MarkerSync>();

                _marker.SetActive(false);

                entity.SetDevice(deviceType);

                EventManager.Players.Add(photonView.Owner.ActorNumber, this);
            }

            _markerSync.Board = GameManager.Instance.Board;

            DontDestroyOnLoad(gameObject);
        }

        public void ReceiveMarkerGrab(object data)
        {
            var grabbed = (bool) data;

            _marker.SetActive(grabbed);
        }

        public void ReceiveMarkerPosition(object data)
        {
            _markerSync.ReceiveTransform(data as object[]);
        }

        public void ReceiveMarkerColor(object data)
        {
            _markerSync.SetColor(data as object[]);
        }
    }
}