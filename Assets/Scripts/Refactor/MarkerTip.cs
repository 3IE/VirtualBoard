using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using EventCode = Utils.EventCode;

namespace Refactor
{
    public class MarkerTip : MonoBehaviour
    {
        [SerializeField] private MarkerSync markerSync;

        private XRBaseController _controller;
        private bool             _touchesBoard;

        private void Update()
        {
            if (_touchesBoard)

                // ReSharper disable once Unity.NoNullPropagation
                _controller?.SendHapticImpulse(0.1f, 0.1f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Board"))
                return;

            PhotonNetwork.RaiseEvent((byte) EventCode.MarkerGrab, true,
                                     new RaiseEventOptions { Receivers = ReceiverGroup.Others },
                                     SendOptions.SendReliable);

            markerSync.StartSend();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Board"))
                return;

            markerSync.StopSend();

            PhotonNetwork.RaiseEvent((byte) EventCode.MarkerGrab, false,
                                     new RaiseEventOptions { Receivers = ReceiverGroup.Others },
                                     SendOptions.SendReliable);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Board"))
                return;

            _touchesBoard = true;
            markerSync.Snap();
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Board"))
                return;

            _touchesBoard = false;
            markerSync.UnSnap();
        }

        public void OnGrab(SelectEnterEventArgs eventArgs)
        {
            IXRSelectInteractor interactor = eventArgs.interactorObject;
            markerSync.controller = interactor.transform;
            _controller           = markerSync.controller.GetComponent<XRBaseController>();
        }

        public void OnRelease()
        {
            markerSync.controller = null;
            _controller           = null;
        }
    }
}