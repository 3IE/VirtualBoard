using UnityEngine;
using DeviceType = Utils.DeviceType;

namespace Users
{
    /// <summary>
    /// Utility class used to hold data about a user
    /// </summary>
    public class PlayerEntity : MonoBehaviour
    {
        /// <summary> username of the user </summary>
        public string username;

        /// <summary> id of the user </summary>
        public int photonId;

        /// <summary> device of the user </summary>
        public DeviceType device;

        /// <summary>
        /// Sets the data of this user
        /// </summary>
        /// <param name="deviceType"> device of the user </param>
        /// <param name="id"> id of the user </param>
        /// <param name="user"> username </param>
        public void SetValues(DeviceType deviceType, int id, string user)
        {
            device = deviceType;
            photonId = id;
            username = user;
        }

        /// <summary>
        /// Updates the position of the player
        /// </summary>
        /// <param name="position"> new position </param>
        public void UpdateTransform(Vector3 position)
        {
            transform.position = position;
        }
    }
}