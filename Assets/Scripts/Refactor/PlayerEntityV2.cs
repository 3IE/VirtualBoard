using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using DeviceType = Utils.DeviceType;
using EventCode = Utils.EventCode;

namespace Refactor
{
    public class PlayerEntityV2 : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private Transform rightHandTransform;

        private Vector3 _playerPos;
        private Vector3 _leftHandPos;
        private Vector3 _rightHandPos;

        private Quaternion _playerRot;
        private Quaternion _leftHandRot;
        private Quaternion _rightHandRot;

        private bool _isAr;
        private bool _isMine;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void FixedUpdate()
        {
            if (!_isMine)
                return;

            var data = new object[]
            {
                playerTransform.position, leftHandTransform.position, rightHandTransform.position,
                playerTransform.rotation, leftHandTransform.rotation, rightHandTransform.rotation,
            };
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte) EventCode.SendNewPosition, data, raiseEventOptions,
                                     SendOptions.SendUnreliable);
        }

        private void Update()
        {
            if (_isMine)
                return;

            playerTransform.position    = _playerPos;
            leftHandTransform.position  = _leftHandPos;
            rightHandTransform.position = _rightHandPos;

            playerTransform.rotation    = _playerRot;
            leftHandTransform.rotation  = _leftHandRot;
            rightHandTransform.rotation = _rightHandRot;
        }

        public void UpdateTransforms(object[] data)
        {
            _playerPos    = (Vector3) data[0];
            _leftHandPos  = (Vector3) data[1];
            _rightHandPos = (Vector3) data[2];

            _playerRot    = (Quaternion) data[3];
            _leftHandRot  = (Quaternion) data[4];
            _rightHandRot = (Quaternion) data[5];
        }

        public void SetDevice(DeviceType deviceType)
        {
            _isAr = deviceType is not DeviceType.VR;
        }

        public void SetOwnership()
        {
            _isMine = true;
        }

        public void ReplaceHandsTransforms(Transform newLeftHandTransform, Transform newRightHandTransform)
        {
            Destroy(leftHandTransform.gameObject);
            Destroy(rightHandTransform.gameObject);

            if (_isAr)
                return;

            leftHandTransform  = newLeftHandTransform;
            rightHandTransform = newRightHandTransform;
        }
    }
}