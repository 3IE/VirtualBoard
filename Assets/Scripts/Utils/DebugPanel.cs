using System;
using System.Globalization;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Utils
{
    /// <summary>
    ///     Singleton class, shows the current state of the session for debug purposes.
    /// </summary>
    /// WARNING: This class is for debugging purposes only and should not be used in production.
    /// TODO: Remove the DEBUG preprocessor directive to remove this object from the build.
    public class DebugPanel : MonoBehaviour
    {
        #if DEBUG
        /// <summary>
        ///     Instance of the class
        /// </summary>
        public static DebugPanel Instance { get; private set; }

        #region GUI

        [SerializeField] private TextMeshProUGUI framerate;
        [SerializeField] private TextMeshProUGUI time;
        [SerializeField] private TextMeshProUGUI connectionTime;
        [SerializeField] private TextMeshProUGUI ping;
        [SerializeField] private TextMeshProUGUI sent;
        [SerializeField] private TextMeshProUGUI received;

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

        #endregion

        #region ATTRIBUTES

        private bool _connected;

        private float _deltaTime;
        private float _pingTime;
        private float _startConnectionTime;

        private int _nbPing;
        private int _nbSent;
        private int _nbReceived;

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

            //InvokeRepeating(nameof(SendPing), .2f, .2f);
        }

        // Update is called once per frame
        private void Update()
        {
            _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
            _pingTime  =  PhotonNetwork.NetworkingClient.LoadBalancingPeer.RoundTripTime;

            float fps = 1.0f / _deltaTime;

            framerate.text = Mathf.Ceil(fps).ToString(CultureInfo.InvariantCulture);
            time.text      = $"{Time.unscaledTime:0.000}";

            if (!_connected)
                connectionTime.text = $"{Time.unscaledTime - _startConnectionTime:0.000}";

            ping.text = $"{_pingTime:0.000}";
        }

        #endregion

        #region SETTERS

        /// <summary>
        ///     Sets <see cref="_connected" /> to the value of <c>connected</c>
        /// </summary>
        /// <param name="connected"> value to assign</param>
        public void SetConnected(bool connected)
        {
            if (!connected)
                _startConnectionTime = Time.unscaledTime;

            _connected = connected;
        }

        /// <summary>
        ///     Updates the debug panel to add a player
        /// </summary>
        /// <param name="deviceType"> type of player to add </param>
        /// <exception cref="ArgumentOutOfRangeException"> Occurs when the given argument is an unknown value </exception>
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

        /// <summary>
        ///     Updates the debug panel to remove a player
        /// </summary>
        /// <param name="deviceType"> type of player to add </param>
        /// <exception cref="ArgumentOutOfRangeException"> Occurs when the given argument is an unknown value </exception>
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

        public void AddSent()
        {
            _nbSent++;
            UpdateSent();
        }

        public void AddReceived()
        {
            _nbReceived++;
            UpdateReceived();
        }

        /// <summary>
        ///     Used to update the debug panel to add a message about a player being sent
        /// </summary>
        public void AddPlayerSent()
        {
            AddSent();
            _playerSentValue++;
            UpdatePlayerSent();
        }

        /// <summary>
        ///     Used to update the debug panel to add a message about a player being received
        /// </summary>
        public void AddPlayerReceived()
        {
            _playerReceivedValue++;
            UpdatePlayerReceived();
        }

        /// <summary>
        ///     Used to update the debug panel to add a message about an object being sent
        /// </summary>
        public void AddObjectSent()
        {
            AddSent();
            _objectSentValue++;
            UpdateObjectSent();
        }

        /// <summary>
        ///     Used to update the debug panel to add a message about an object being received
        /// </summary>
        public void AddObjectReceived()
        {
            _objectReceivedValue++;
            UpdateObjectReceived();
        }

        /// <summary>
        ///     Used to update the debug panel to add an object
        /// </summary>
        public void AddObject()
        {
            _objectNbValue++;
            UpdateObjectNb();
        }

        /// <summary>
        ///     Used to update the debug panel to remove an object
        /// </summary>
        public void RemoveObject()
        {
            _objectNbValue--;
            UpdateObjectNb();
        }

        /// <summary>
        ///     Used to update the debug panel to add a custom object
        /// </summary>
        public void AddCustom()
        {
            _customNbValue++;
            UpdateCustomNb();
        }

        /// <summary>
        ///     Used to update the debug panel to remove a custom object
        /// </summary>
        public void RemoveCustom()
        {
            _customNbValue--;
            UpdateCustomNb();
        }

        /// <summary>
        ///     Used to update the debug panel to add a message about the board being sent
        /// </summary>
        public void AddBoardSent()
        {
            AddSent();
            _boardSentValue++;
            UpdateBoardSent();
        }

        /// <summary>
        ///     Used to update the debug panel to add a message about the board being received
        /// </summary>
        public void AddBoardReceived()
        {
            _boardReceivedValue++;
            UpdateBoardReceived();
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

        private void UpdateSent()
        {
            sent.text = _nbSent.ToString();
        }

        private void UpdateReceived()
        {
            received.text = _nbReceived.ToString();
        }

        private void UpdatePlayerSent()
        {
            //playerSent.text = _playerSentValue.ToString();
        }

        private void UpdatePlayerReceived()
        {
            //playerReceived.text = _playerReceivedValue.ToString();
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

        #endregion

        #else
    private void Awake()
    {
        Destroy(gameObject);
    }

        #endif
    }
}