using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using UnityEngine;

namespace Board.Chat
{
    /// <inheritdoc cref="Photon.Chat.IChatClientListener" />
    public class Chat : MonoBehaviour, IChatClientListener
    {
        [SerializeField] private string chatAppId;
        [SerializeField] private string chatAppVersion;

        private ChatClient           _chatClient;
        private bool                 _connected;
        private AuthenticationValues _id;

        private List<string> _msgList;

        private string _userId;

        private void Start()
        {
            if (_chatClient is not null)
                return;

            _chatClient = new ChatClient(this)
            {
                ChatRegion = "EU",
            };

            _msgList = new List<string>(4);
            _id      = new AuthenticationValues(PhotonNetwork.LocalPlayer.UserId);

            _chatClient.Connect(chatAppId, chatAppVersion, _id);
        }

        private void Update()
        {
            if (_connected)
                _chatClient.PublishMessage("global", "Hello world!");
        }

        private void OnEnable()
        {
            InvokeRepeating(nameof(CallService), .5f, 1f);
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        private void CallService()
        {
            _chatClient.Service();
        }

        #region IChatClientListener

        /// <inheritdoc />
        public void DebugReturn(DebugLevel level, string message)
        {
            switch (level)
            {
                case DebugLevel.ERROR:
                    Debug.LogError(message);
                    break;

                case DebugLevel.WARNING:
                    Debug.LogWarning(message);
                    break;

                case DebugLevel.OFF:
                case DebugLevel.INFO:
                case DebugLevel.ALL:
                default:
                    Debug.Log(message);
                    break;
            }
        }

        /// <inheritDoc />
        public void OnDisconnected()
        {
            _connected = false;
            _chatClient.Unsubscribe(new[] { "global" });

            Debug.Log("Disconnected from chat server");
        }

        /// <inheritDoc />
        public void OnConnected()
        {
            _connected = true;
            _userId    = _id.UserId;
            _chatClient.Subscribe(new[] { "global" });

            Debug.Log("Connected to chat server");
        }

        /// <inheritDoc />
        public void OnChatStateChange(ChatState state)
        {
            // Do nothing
        }

        /// <inheritDoc />
        public void OnGetMessages(string channelName, string[] senders, object[] messages)
        {
            for (var i = 0; i < senders.Length; i++)
            {
                if (!string.Equals(senders[i], _userId))
                    _msgList.Add($"{senders[i]}={messages[i]}");
            }

            if (_msgList.Count == 0) return;

            Debug.Log($"OnGetMessages: {channelName} ({senders.Length}) > {string.Join(", ", _msgList)}");
            _msgList.Clear();
        }

        /// <inheritDoc />
        public void OnPrivateMessage(string sender, object message, string channelName)
        {
            Debug.LogFormat("OnPrivateMessage: {0} ({1}) > {2}", channelName, sender,
                            message);
        }

        /// <inheritDoc />
        public void OnSubscribed(string[] channels, bool[] results)
        {
            Debug.Log($"OnSubscribed: {string.Join(", ", channels)}");
        }

        /// <inheritDoc />
        public void OnUnsubscribed(string[] channels)
        {
            Debug.Log($"OnUnsubscribed: {string.Join(", ", channels)}");
        }

        /// <inheritDoc />
        public void OnStatusUpdate(string user, int status, bool gotMessage,
                                   object message)
        {
            // Do nothing
        }

        /// <inheritDoc />
        public void OnUserSubscribed(string channel, string user)
        {
            Debug.LogFormat("OnUserSubscribed: channel=\"{0}\" userId=\"{1}\"", channel, user);
        }

        /// <inheritDoc />
        public void OnUserUnsubscribed(string channel, string user)
        {
            Debug.LogFormat("OnUserUnsubscribed: channel=\"{0}\" userId=\"{1}\"", channel, user);
        }

        #endregion
    }
}