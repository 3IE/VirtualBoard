using System;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using Utils;
using Event = Utils.Event;

namespace Users
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

        private XRIDefaultInputActions _customInputs;
        #region ressource
        // https://forum.unity.com/threads/fetch-xr-handed-ness-from-an-inputaction.907139/
        #endregion
        
        [SerializeField] 
        private Transform rightHandTransform;
        [SerializeField] 
        private Transform leftHandTransform;

        private void Awake()
        {
            _customInputs = new XRIDefaultInputActions();
        }

        private void OnEnable()
        {
            _customInputs.Enable();
            _customInputs.customAction.Ping.started += TryPing;
            _vrCamTransform = Camera.main!.transform;
            InvokeRepeating(nameof(SendNewPositionEvent), refreshRate, refreshRate);
        }

        private void TryPing(InputAction.CallbackContext obj)
        {
            print($"Trying ping");
            Ray ray = new (rightHandTransform.position, rightHandTransform.forward);
            if (!Physics.Raycast(ray, out var hit, 100f)) return;
            if (hit.collider.CompareTag("Board"))
                Ping(hit.point);
            else if (hit.collider.CompareTag("Ping"))
                Destroy(hit.collider.gameObject.transform.parent.gameObject);
        }

        private void OnDisable() 
        {
            CancelInvoke();
            _customInputs.customAction.Ping.started -= TryPing;
            _customInputs.Disable();
        }
        private void SendNewPositionEvent()
        {
            object[] content = { _vrCamTransform.position - boardTransform.position };
        
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte) Event.EventCode.SendNewPosition, content, raiseEventOptions, SendOptions.SendReliable);
        }
        private void Ping(Vector3 position)
        { 
            // pooling des ping, 2 prefab de ping (un pour l'utilisateur et un pour les autres) 
            // ping physique
            var ping = Instantiate(localPingPrefab,  position, boardTransform.rotation, boardTransform);
            // ping sur le reseaux
            var localPos = ping.transform.localPosition;
            SendNewPingEvent(new Vector2(localPos.x, localPos.y));
        }
        private void SendNewPingEvent(Vector2 position) {
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            
            PhotonNetwork.RaiseEvent((byte) Event.EventCode.SendNewPing, position, raiseEventOptions, SendOptions.SendReliable);
        }
    
        /// <summary>
        /// TODO: DELETE!
        /// </summary>
        public void HahaPinged()
        {
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte) Event.EventCode.SendNewPing, Vector2.one, raiseEventOptions, SendOptions.SendReliable);
        }
    }
}
