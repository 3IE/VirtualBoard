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

        [SerializeField] private Transform board;
        [SerializeField] private Transform tip;
        [SerializeField] private Transform trace;

        private bool _send;
        private bool _empty;

        private float _factor;
        private float _boardMaxDistance;

        private Transform _transform;

        private Vector3    _lastPosition;
        private Vector3    _lastSentPosition;
        private Quaternion _lastSentRotation;

        private Queue<Vector3>    _grabPosition;
        private Queue<Quaternion> _grabRotation;

        private Material _material;
        private Material _traceMaterial;

        public Transform Board
        {
            set { board = value; }
        }

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
            _transform    = transform;
            _lastPosition = _transform.position;

            float scale = _transform.localScale.z;

            _factor           = 1     / scale / tip.transform.localScale.z;
            _boardMaxDistance = scale * tip.localPosition.z;

            _material      = GetComponent<Renderer>().material;
            _traceMaterial = trace.GetComponent<Renderer>().material;

            _grabPosition = new Queue<Vector3>();
            _grabRotation = new Queue<Quaternion>();

            LocalInstance ??= this;
        }

        private void Update()
        {
            if (_send)
                SendTransform();
            else
                CheckIfNewPosition();

            UpdateTrace();

            _lastPosition = tip.position;
        }

        private void CheckIfNewPosition()
        {
            if (_empty)
                return;

            if (_grabPosition.TryDequeue(out Vector3 position))
                _transform.position = position;

            if (_grabRotation.TryDequeue(out Quaternion rotation))
                _transform.rotation = rotation;

            _empty = _grabPosition.Count == 0 && _grabRotation.Count == 0;
        }

        private void UpdateTrace()
        {
            Vector3 tipPosition = tip.position;
            Vector3 distance    = tipPosition - _lastPosition;
            float   length      = distance.magnitude;

            if (length == 0)
                return;

            Vector3 scale         = trace.localScale;
            float   boardDistance = Mathf.Abs(board.position.z - tipPosition.z);

            boardDistance =  Mathf.Clamp(boardDistance, 0f, _boardMaxDistance);
            boardDistance /= _boardMaxDistance;

            scale.x = 1 - boardDistance;
            scale.z = length * _factor;

            trace.position   = _lastPosition + distance / 2;
            trace.localScale = scale;
            trace.LookAt(tip);
        }

        private void SendTransform()
        {
            if ((_transform.position - _lastSentPosition).sqrMagnitude      < 0.01f
                && Quaternion.Angle(_transform.rotation, _lastSentRotation) < 0.1f)
                return;

            _lastSentPosition = _transform.position;
            _lastSentRotation = _transform.rotation;

            object[] data = { _lastSentPosition, _lastSentRotation };

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

            _material.color      = color;
            _traceMaterial.color = color;
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