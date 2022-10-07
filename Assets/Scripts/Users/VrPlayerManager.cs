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
            _customInputs = new XRIDefaultInputActions();
            
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

        private void OpenMenu(InputAction.CallbackContext obj)
        {
            vrMenu.transform.SetPositionAndRotation(_vrCamTransform.position, _vrCamTransform.rotation);
            vrMenu.transform.Translate(_vrCamTransform.forward * 1.5f);
            vrMenu.gameObject.SetActive(true);
            
            vrMenu.WakeUp();
        }

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