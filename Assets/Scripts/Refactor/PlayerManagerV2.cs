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

        [SerializeField] private DeviceType deviceType;

        private void Awake()
        {
            var photonView = GetComponent<PhotonView>();

            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
                
                entity.SetOwnership();
            }
            else
            {
                deviceType = photonView.Owner.CustomProperties.TryGetValue("Device", out object type)
                    ? (DeviceType) type
                    : DeviceType.Unknown;

                entity.SetDevice(deviceType);
            }

            DontDestroyOnLoad(gameObject);
        }
    }
}