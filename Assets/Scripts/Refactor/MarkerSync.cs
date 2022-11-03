using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using EventCode = Utils.EventCode;

namespace Refactor
{
    public class MarkerSync : MonoBehaviour
    {
        public static MarkerSync LocalInstance;

        private bool _send;
        private bool _empty;

        private Transform _transform;

        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        
        private Queue<Vector3>    _grabPosition;
        private Queue<Quaternion> _grabRotation;

        private Material _material;

        public void SendColor()
        {
            Color    color = _material.color;
            object[] data  = { color.r, color.g, color.b, color.a };

            PhotonNetwork.RaiseEvent((byte) EventCode.MarkerColor, data,
                                     new RaiseEventOptions { Receivers = ReceiverGroup.Others },
                                     SendOptions.SendUnreliable);
        }

        private void Awake()
        {
            _transform = transform;
            _material  = GetComponent<Renderer>().material;
            _grabPosition = new Queue<Vector3>();
            _grabRotation = new Queue<Quaternion>();

            LocalInstance ??= this;
        }

        private void Update()
        {
            if (_send)
            {
                SendTransform();
                return;
            }
            
            if (_empty)
                return;

            if (_grabPosition.TryDequeue(out Vector3 position))
                _transform.position = position;
            
            if (_grabRotation.TryDequeue(out Quaternion rotation))
                _transform.rotation = rotation;
            
            _empty = _grabPosition.Count == 0 && _grabRotation.Count == 0;
        }

        private void SendTransform()
        {
            if ((_transform.position - _lastPosition).sqrMagnitude < 0.01f &&
                Quaternion.Angle(_transform.rotation, _lastRotation) < 0.1f)
                return;

            _lastPosition = _transform.position;
            _lastRotation = _transform.rotation;

            object[] data = { _lastPosition, _lastRotation };

            PhotonNetwork.RaiseEvent((byte) EventCode.MarkerPosition, data,
                                     new RaiseEventOptions { Receivers = ReceiverGroup.Others },
                                     SendOptions.SendReliable);
        }

        public void ReceiveTransform(object[] data)
        {
            _grabPosition.Enqueue((Vector3) data[0]);
            _grabRotation.Enqueue((Quaternion) data[1]);

            _empty = false;
        }

        public void SetColor(object[] data)
        {
            var color = new Color((float) data[0], (float) data[1], (float) data[2],
                                  (float) data[3]);

            _material.color = color;
        }

        public void StartSend()
        {
            _send = true;
        }
        
        public void StopSend()
        {
            _send = false;
        }
    }
}