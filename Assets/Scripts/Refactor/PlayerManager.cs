using Photon.Pun;
using UnityEngine;
using DeviceType = Utils.DeviceType;

namespace Refactor
{
    public class PlayerManager : MonoBehaviour
    {
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        public PlayerEntity entity;

        [SerializeField] private DeviceType deviceType;

        private void Awake()
        {
            var photonView = GetComponent<PhotonView>();

            if (photonView.IsMine)
                LocalPlayerInstance = this.gameObject;
            else
            {
                deviceType = photonView.Owner.CustomProperties.TryGetValue("Device", out object type)
                    ? (DeviceType) type
                    : DeviceType.Unknown;

                entity.SetDevice(this.deviceType);
            }

            DontDestroyOnLoad(this.gameObject);
        }
    }
}