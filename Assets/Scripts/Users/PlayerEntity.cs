using UnityEngine;
using DeviceType = Utils.DeviceType;

namespace Users
{
    public class PlayerEntity : MonoBehaviour
    {
        public string username;
        public int photonId;
        public DeviceType device;

        public void SetValues(DeviceType deviceType, int id, string user)
        {
            device = deviceType;
            photonId = id;
            username = user;
        }

        public void UpdateTransform(Vector3 position)
        {
            transform.position = position;
        }
    }
}