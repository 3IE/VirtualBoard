using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Utils;
using DeviceType = Utils.DeviceType;
using Event = Utils.Event;

// https://forum.unity.com/threads/fetch-xr-handed-ness-from-an-inputaction.907139/

namespace Users
{
    public class VrPlayerManager : MonoBehaviour
    {
        // to attach to XR Origin/Camera Offset
        [SerializeField] [Range(0.01f, 1.0f)] private float refreshRate = 0.1f;
        [SerializeField] private Transform boardTransform;
        [SerializeField] private GameObject localPingPrefab;

        [SerializeField] private Transform rightHandTransform;
        [SerializeField] private Transform leftHandTransform;

        [SerializeField] private VRMenu vrMenu;

        private Transform _vrCamTransform;

        private XRIDefaultInputActions _customInputs;

        private void Awake()
        {
            if (Unity.XR.Oculus.Performance.TrySetDisplayRefreshRate(90f))
                Debug.Log("Could not set display refresh rate to 90Hz !");
            
            _customInputs = new XRIDefaultInputActions();
        }

        private void Start()
        {
            DebugPanel.Instance.AddPlayer(DeviceType.VR);
        }

        private void OnEnable()
        {
            _customInputs.Enable();
            _customInputs.OnlineInteractions.Ping.started += TryPing;
            _customInputs.Menu.OpenMenu.started += OpenMenu;

            _vrCamTransform = Camera.main!.transform;

            InvokeRepeating(nameof(SendNewPositionEvent), refreshRate, refreshRate);
        }

        /// <summary>
        /// Opens the menu in front of the player, or closes it if it's already open
        /// </summary>
        /// <param name="obj"></param>
        private void OpenMenu(InputAction.CallbackContext obj)
        {
            if (vrMenu.gameObject.activeSelf)
                vrMenu.Close();
            else
            {
                vrMenu.transform.SetPositionAndRotation(_vrCamTransform.position, _vrCamTransform.rotation);
                vrMenu.transform.Translate(_vrCamTransform.forward * 1.5f);
                vrMenu.gameObject.SetActive(true);
                
                vrMenu.Open();
            }
        }

        /// <summary>
        /// Tries to place a ping on the board from the player's hand,
        /// if the player pings a ping, it'll delete this ping
        /// </summary>
        private void TryPing(InputAction.CallbackContext obj)
        {
            Ray ray = new(rightHandTransform.position, rightHandTransform.forward);

            if (!Physics.Raycast(ray, out var hit, 100f)) return;

            if (hit.collider.CompareTag("Board"))
                Ping(hit.point);
            else if (hit.collider.CompareTag("Ping"))
                Destroy(hit.collider.gameObject.transform.parent.gameObject);
        }

        private void OnDisable()
        {
            CancelInvoke();

            _customInputs.OnlineInteractions.Ping.started -= TryPing;
            _customInputs.Menu.OpenMenu.started -= OpenMenu;
            _customInputs.Disable();
        }

        private void SendNewPositionEvent()
        {
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte)Event.EventCode.SendNewPosition, _vrCamTransform.position, raiseEventOptions,
                SendOptions.SendReliable);
            
#if DEBUG
            DebugPanel.Instance.AddPlayerSent();
#endif
        }

        /// <summary>
        /// Places a ping on the board and declare its position to the other players
        /// </summary>
        /// <param name="position">Position of the ping</param>
        private void Ping(Vector3 position)
        {
            // instantiate ping
            var ping = Instantiate(localPingPrefab, position, boardTransform.rotation, boardTransform);

            // send ping to other players
            var localPos = ping.transform.localPosition;

            SendNewPingEvent(new Vector2(localPos.x, localPos.y));
        }

        private static void SendNewPingEvent(Vector2 position)
        {
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte)Event.EventCode.SendNewPing, position, raiseEventOptions,
                SendOptions.SendReliable);

#if DEBUG
            DebugPanel.Instance.AddPlayerSent();
#endif
        }
    }
}