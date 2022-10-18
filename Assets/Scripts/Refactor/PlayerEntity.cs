using Photon.Pun;
using UnityEngine;
using DeviceType = Utils.DeviceType;

namespace Refactor
{
    public class PlayerEntity : MonoBehaviour, IPunObservable
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private Transform rightHandTransform;

        private bool _isAr;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void SetDevice(DeviceType deviceType)
        {
            _isAr = deviceType is not DeviceType.VR;
        }

        public void ReplaceHandsTransforms(Transform newLeftHandTransform, Transform newRightHandTransform)
        {
            Destroy(leftHandTransform.gameObject);
            Destroy(rightHandTransform.gameObject);
            
            if (_isAr)
                return;
                
            this.leftHandTransform = newLeftHandTransform;
            this.rightHandTransform = newRightHandTransform;
        }
        
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo messageInfo)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(playerTransform.position);
                stream.SendNext(playerTransform.rotation);
                
                stream.SendNext(leftHandTransform.position);
                stream.SendNext(leftHandTransform.rotation);
                
                stream.SendNext(rightHandTransform.position);
                stream.SendNext(rightHandTransform.rotation);
            }
            else
            {
                playerTransform.position = (Vector3) stream.ReceiveNext();
                playerTransform.rotation = (Quaternion) stream.ReceiveNext();
                
                if (_isAr)
                    return;
                
                leftHandTransform.position = (Vector3) stream.ReceiveNext();
                leftHandTransform.rotation = (Quaternion) stream.ReceiveNext();
                
                rightHandTransform.position = (Vector3) stream.ReceiveNext();
                rightHandTransform.rotation = (Quaternion) stream.ReceiveNext();
            }
        }
    }
}