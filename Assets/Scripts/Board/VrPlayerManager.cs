using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;
using Event = Utils.Event;

namespace Board
{
    public class VrPlayerManager : MonoBehaviour
    {
        // a attacher au XR Origin/Camera Offset
        private Transform _vrCamTransform;
        [FormerlySerializedAs("BoardTransform")] [SerializeField] 
        private Transform boardTransform;
        [FormerlySerializedAs("_refreshRate")] [SerializeField] 
        private float refreshRate = 0.2f;
        [SerializeField]
        private GameObject localPingPrefab;

        private void OnEnable()
        {
            _vrCamTransform = Camera.main!.transform;
            InvokeRepeating(nameof(SendNewPositionEvent), refreshRate, refreshRate);
        }
        private void OnDisable() => CancelInvoke();
        private void SendNewPositionEvent()
        {
            object[] content = { _vrCamTransform.position - boardTransform.position };
        
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte) Event.EventCode.SendNewPosition, content, raiseEventOptions, SendOptions.SendReliable);
        }
        private void Ping(Vector3 position)
        { // pooling des ping, 2 prefab de ping (un pour l'utilisateur et un pour les autres) 
            // ping physique
            var ping = Instantiate(localPingPrefab,  position, boardTransform.rotation, boardTransform);
            // ping sur le reseaux
            var localPos = ping.transform.localPosition;
            SendNewPingEvent(new Vector2(localPos.x, localPos.y));
        }
        private void SendNewPingEvent(Vector2 position) {
            object[] content = { position };
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent((byte) Event.EventCode.SendNewPing, content, raiseEventOptions, SendOptions.SendReliable);
        }
    
        //! TO DELETE LATER
        public void HahaPinged()
        {
            object[] content = { new Vector2(1, 1)};
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent((byte) Event.EventCode.SendNewPing, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }
}
