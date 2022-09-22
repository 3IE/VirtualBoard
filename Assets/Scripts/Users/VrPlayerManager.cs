using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Event = Utils.Event;

namespace Board
{
    public class VrPlayerManager : MonoBehaviour
    {
        // a attacher au XR Origin/Camera Offset
        private Transform _vrCamTransform;
        [SerializeField] private Transform boardTransform;
        [SerializeField] private float refreshRate = 0.2f;
        [SerializeField] private GameObject localPingPrefab;

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

            PhotonNetwork.RaiseEvent((byte)Event.EventCode.SendNewPosition, content, raiseEventOptions,
                SendOptions.SendReliable);
        }

        private void Ping(Vector3 position)
        {
            // pooling des ping, 2 prefab de ping (un pour l'utilisateur et un pour les autres) 
            // ping physique
            var ping = Instantiate(localPingPrefab, position, boardTransform.rotation, boardTransform);
            // ping sur le reseaux
            var localPos = ping.transform.localPosition;
            SendNewPingEvent(new Vector2(localPos.x, localPos.y));
        }

        private void SendNewPingEvent(Vector2 position)
        {
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte)Event.EventCode.SendNewPing, position, raiseEventOptions,
                SendOptions.SendReliable);
        }

        /// <summary>
        /// TODO: DELETE!
        /// </summary>
        public void HahaPinged()
        {
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte)Event.EventCode.SendNewPing, Vector2.one, raiseEventOptions,
                SendOptions.SendReliable);
        }
    }
}