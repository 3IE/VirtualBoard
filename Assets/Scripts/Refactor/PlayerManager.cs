using Photon.Pun;
using UnityEngine;
using DeviceType = Utils.DeviceType;

namespace Refactor
{
    public class PlayerManager : MonoBehaviour
    {
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        private DeviceType _deviceType;

        private void Awake()
        {
            var photonView = GetComponent<PhotonView>();
        
            if (photonView.IsMine)
                LocalPlayerInstance = this.gameObject;

            _deviceType = photonView.Owner.CustomProperties.TryGetValue("DeviceType", out object deviceType)
                ? (DeviceType) deviceType
                : DeviceType.Unknown;

            DontDestroyOnLoad(this.gameObject);
        }
    }
}