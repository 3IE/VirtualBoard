using System;
using UnityEngine;
using DeviceType = Utils.DeviceType;

namespace Users
{
    /// <summary>
    ///     Utility class used to hold data about a user
    /// </summary>
    public class PlayerEntity : MonoBehaviour
    {
        /// <summary> username of the user </summary>
        public string username;

        /// <summary> id of the user </summary>
        public int photonId;

        /// <summary> device of the user </summary>
        public DeviceType device;

        [SerializeField] private GameObject player;
        [SerializeField] private GameObject leftHand;
        [SerializeField] private GameObject rightHand;
        private                  Transform  _leftTransform;
        private                  Transform  _rightTransform;

        private Transform _transform;

        private void Awake()
        {
            _transform      = player.transform;
            _leftTransform  = leftHand  != null ? leftHand.transform : null;
            _rightTransform = rightHand != null ? rightHand.transform : null;
        }

        /// <summary>
        ///     Sets the data of this user
        /// </summary>
        /// <param name="deviceType"> device of the user </param>
        /// <param name="id"> id of the user </param>
        /// <param name="user"> username </param>
        public PlayerEntity SetValues(DeviceType deviceType, int id, string user)
        {
            device   = deviceType;
            photonId = id;
            username = user;

            return this;
        }

        /// <summary>
        ///     Updates the position of the player
        /// </summary>
        /// <param name="position"> new position </param>
        /// <param name="rotation"> new rotation </param>
        private void UpdateTransform(Vector3 position, Quaternion rotation = default)
        {
            _transform.position = position;
            _transform.rotation = rotation;
        }

        private void UpdateHands(Vector3    left, Quaternion leftRot, Vector3 right,
                                 Quaternion rightRot)
        {
            _leftTransform.position  = left;
            _leftTransform.rotation  = leftRot;
            _rightTransform.position = right;
            _rightTransform.rotation = rightRot;
        }

        public void UpdateObject(object data)
        {
            if (data is not object[] dataArray)
                throw new InvalidCastException("Data is not an array");

            var position = (Vector3) dataArray[0];
            var rotation = (Quaternion) dataArray[1];

            UpdateTransform(position, rotation);

            var leftHandPosition  = (Vector3) dataArray[2];
            var leftHandRotation  = (Quaternion) dataArray[3];
            var rightHandPosition = (Vector3) dataArray[4];
            var rightHandRotation = (Quaternion) dataArray[5];

            UpdateHands(leftHandPosition, leftHandRotation, rightHandPosition,
                        rightHandRotation);
        }
    }
}