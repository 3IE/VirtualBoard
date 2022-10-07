using System;
using System.Globalization;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Singleton class, shows the current state of the session for debug purposes.
    /// </summary>
    /// WARNING: This class is for debugging purposes only and should not be used in production.
    /// TODO: Remove the DEBUG preprocessor directive to remove this object from the build.
    public class DebugPanel : MonoBehaviour
    {

#if DEBUG
        public static DebugPanel Instance { get; private set; }

        #region GUI

        [SerializeField] private TextMeshProUGUI framerate;
        [SerializeField] private TextMeshProUGUI time;
        [SerializeField] private TextMeshProUGUI connectionTime;
        [SerializeField] private TextMeshProUGUI ping;

        [SerializeField] private TextMeshProUGUI vrNb;
        [SerializeField] private TextMeshProUGUI arNb;
        [SerializeField] private TextMeshProUGUI holoNb;

        [SerializeField] private TextMeshProUGUI playerSent;
        [SerializeField] private TextMeshProUGUI playerReceived;

        [SerializeField] private TextMeshProUGUI objectSent;
        [SerializeField] private TextMeshProUGUI objectReceived;

        [SerializeField] private TextMeshProUGUI objectNb;
        [SerializeField] private TextMeshProUGUI customNb;

        [SerializeField] private TextMeshProUGUI boardSent;
        [SerializeField] private TextMeshProUGUI boardReceived;

        [SerializeField] private TextMeshProUGUI boardQueue;

        #endregion

        #region ATTRIBUTES

        private bool _connected;

        private float _deltaTime;
        private float _pingTime;
        private float _startConnectionTime;

        private int _nbPing;

        private int _vrNbValue;
        private int _arNbValue;
        private int _holoNbValue;

        private int _playerSentValue;
        private int _playerReceivedValue;

        private int _objectSentValue;
        private int _objectReceivedValue;

        private int _objectNbValue;
        private int _customNbValue;

        private int _boardSentValue;
        private int _boardReceivedValue;

        private int _boardQueueValue;

        #endregion

        #region UNITY

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetConnected(false);
            InvokeRepeating(nameof(SendPing), .2f, .2f);
        }

        // Update is called once per frame
        private void Update()
        {
            _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;

            var fps = 1.0f / _deltaTime;

            framerate.text = Mathf.Ceil(fps).ToString(CultureInfo.InvariantCulture);
            time.text = $"{Time.unscaledTime:0.000}";
            if (!_connected)
                connectionTime.text = $"{Time.unscaledTime - _startConnectionTime:0.000}";
        }

        #endregion

        #region PING

        private void SendPing()
        {
            var pingTime = Time.unscaledTime;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte)Event.EventCode.PingSend, pingTime, raiseEventOptions,
                SendOptions.SendReliable);
        }

        public static void AnswerPing(object data, EventData photonEvent)
        {
            var raiseEventOptions = new RaiseEventOptions { TargetActors = new[] { photonEvent.Sender } };

            PhotonNetwork.RaiseEvent((byte)Event.EventCode.PingSend, data, raiseEventOptions,
                SendOptions.SendReliable);
        }

        public void OnPingReceive(object data)
        {
            var sendTime = (float)data;
            var receiveTime = Time.unscaledTime;

            var pingTime = (receiveTime - sendTime) * 1000;

            _pingTime *= _nbPing++;
            _pingTime += pingTime;
            _pingTime /= _nbPing;

            UpdatePingTime();
        }

        #endregion

        #region SETTERS

        public void SetConnected(bool connected)
        {
            if (!connected)
                _startConnectionTime = Time.unscaledTime;

            _connected = connected;
        }

        public void AddPlayer(DeviceType deviceType)
        {
            switch (deviceType)
            {
                case DeviceType.VR:
                    AddVrPlayer();
                    break;
                case DeviceType.AR:
                    AddArPlayer();
                    break;
                case DeviceType.Hololens:
                    AddHoloPlayer();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
            }
        }

        public void RemovePlayer(DeviceType deviceType)
        {
            switch (deviceType)
            {
                case DeviceType.VR:
                    RemoveVrPlayer();
                    break;
                case DeviceType.AR:
                    RemoveArPlayer();
                    break;
                case DeviceType.Hololens:
                    RemoveHoloPlayer();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
            }
        }

        private void AddVrPlayer()
        {
            _vrNbValue++;
            UpdateVrNb();
        }

        private void RemoveVrPlayer()
        {
            _vrNbValue--;
            UpdateVrNb();
        }

        private void AddArPlayer()
        {
            _arNbValue++;
            UpdateArNb();
        }

        private void RemoveArPlayer()
        {
            _arNbValue--;
            UpdateArNb();
        }

        private void AddHoloPlayer()
        {
            _holoNbValue++;
            UpdateHoloNb();
        }

        private void RemoveHoloPlayer()
        {
            _holoNbValue--;
            UpdateHoloNb();
        }

        public void AddPlayerSent()
        {
            _playerSentValue++;
            UpdatePlayerSent();
        }

        public void AddPlayerReceived()
        {
            _playerReceivedValue++;
            UpdatePlayerReceived();
        }

        public void AddObjectSent()
        {
            _objectSentValue++;
            UpdateObjectSent();
        }

        public void AddObjectReceived()
        {
            _objectReceivedValue++;
            UpdateObjectReceived();
        }

        public void AddObject()
        {
            _objectNbValue++;
            UpdateObjectNb();
        }

        public void RemoveObject()
        {
            _objectNbValue--;
            UpdateObjectNb();
        }

        public void AddCustom()
        {
            _customNbValue++;
            UpdateCustomNb();
        }

        public void RemoveCustom()
        {
            _customNbValue--;
            UpdateCustomNb();
        }

        public void AddBoardSent()
        {
            _boardSentValue++;
            UpdateBoardSent();
        }

        public void AddBoardReceived()
        {
            _boardReceivedValue++;
            UpdateBoardReceived();
        }

        public void AddBoardQueue()
        {
            _boardQueueValue++;
            UpdateBoardQueue();
        }

        public void RemoveBoardQueue()
        {
            _boardQueueValue--;
            UpdateBoardQueue();
        }

        #endregion

        #region TEXT

        private void UpdatePingTime()
        {
            ping.text = $"{_pingTime:0.000}";
        }

        private void UpdateVrNb()
        {
            vrNb.text = _vrNbValue.ToString();
        }

        private void UpdateArNb()
        {
            arNb.text = _arNbValue.ToString();
        }

        private void UpdateHoloNb()
        {
            holoNb.text = _holoNbValue.ToString();
        }

        private void UpdatePlayerSent()
        {
            playerSent.text = _playerSentValue.ToString();
        }

        private void UpdatePlayerReceived()
        {
            playerReceived.text = _playerReceivedValue.ToString();
        }

        private void UpdateObjectSent()
        {
            objectSent.text = _objectSentValue.ToString();
        }

        private void UpdateObjectReceived()
        {
            objectReceived.text = _objectReceivedValue.ToString();
        }

        private void UpdateObjectNb()
        {
            objectNb.text = _objectNbValue.ToString();
        }

        private void UpdateCustomNb()
        {
            customNb.text = _customNbValue.ToString();
        }

        private void UpdateBoardSent()
        {
            boardSent.text = _boardSentValue.ToString();
        }

        private void UpdateBoardReceived()
        {
            boardReceived.text = _boardReceivedValue.ToString();
        }

        private void UpdateBoardQueue()
        {
            boardQueue.text = _boardQueueValue.ToString();
        }

        #endregion

#else
    
    private void Awake()
    {
        Destroy(gameObject);
    }
    
#endif
    }
}