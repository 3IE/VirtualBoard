using UnityEngine;
using DeviceType = Utils.DeviceType;

namespace Board
{
    public class PlayerEntity : MonoBehaviour
    {
        public string username;
        public int photonId;
        public DeviceType device;

        public void SetValues(DeviceType device, int photonId, string username)
        {
            this.device = device;
            this.photonId = photonId;
            this.username = username;
        }

        public void UpdateTransform(Vector3 position)
        {
            transform.position = position;
            //this.transform.rotation = transform.rotation;
        }
    }
}