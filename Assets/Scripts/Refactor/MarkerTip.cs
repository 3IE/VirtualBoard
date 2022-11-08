using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using EventCode = Utils.EventCode;

namespace Refactor
{
    public class MarkerTip : MonoBehaviour
    {
        [SerializeField] private MarkerSync markerSync;

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
    }
}