using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Board.Events
{
    /// <summary>
    ///     Utility class used to delegate the events concerning players <seealso cref="EventManager" />
    /// </summary>
    public static class PlayerEvents
    {
        private static GameObject _postItPrefab;
        private static GameObject _onlinePingPrefab;
        private static GameObject _board;

        /// <summary>
        ///     List of currently existing pings
        /// </summary>
        public static readonly List<GameObject> Pings = new();

        /// <summary>
        ///     List of currently existing post-its
        /// </summary>
        public static readonly List<GameObject> PostIts = new();

        /// <summary>
        ///     Method used to initialize some properties
        /// </summary>
        /// <param name="postItPrefab"> prefab used to instantiate post-its </param>
        /// <param name="onlinePingPrefab"> prefab used to instantiate pings </param>
        /// <param name="board"> game object holding the <see cref="Board" /> script </param>
        public static void SetupPrefabs(GameObject postItPrefab, GameObject onlinePingPrefab, GameObject board)
        {
            _postItPrefab     = postItPrefab;
            _onlinePingPrefab = onlinePingPrefab;
            _board            = board;
        }

        /// <summary>
        ///     Called when a new post-it is received
        /// </summary>
        /// <param name="data"> array holding the position and the text of the post-it </param>
        public static void ReceiveNewPostIt(object[] data)
        {
            var        position    = (Vector2) data[0];
            var        text        = (string) data[1];
            GameObject postIt      = Object.Instantiate(_postItPrefab, _board.transform);
            float      invertScale = 1 / _board.transform.localScale.x;

            postIt.transform.localPosition =  new Vector3(position.x * invertScale, 0.01f, position.y * invertScale);
            postIt.transform.rotation      =  Quaternion.identity;
            postIt.transform.localScale    *= invertScale;

            postIt.GetComponentInChildren<TMP_Text>().text = text;

            // TODO: change color depending on the player who created the post it
            postIt.GetComponentInChildren<Renderer>().material.color = Color.cyan;

            PostIts.Add(postIt);
        }

        /// <summary>
        ///     Called when a new ping is received
        /// </summary>
        /// <param name="position"> position of the ping </param>
        public static void ReceivePing(Vector2 position)
        {
            GameObject ping        = Object.Instantiate(_onlinePingPrefab, _board.transform);
            float      invertScale = 1 / _board.transform.localScale.x;

            ping.transform.localPosition =  new Vector3(position.x * invertScale, 0.01f, position.y * invertScale);
            ping.transform.rotation      =  Quaternion.identity;
            ping.transform.localScale    *= invertScale;

            Pings.Add(ping);

            /*
             TODO: add PingSearcher
             _currentPing = ping.transform.position;
             _pingSearcher.AssignedPing = ping;
             _pingSearcher.gameObject.SetActive(true);
            */
        }

        /// <summary>
        ///     Removes all pings and post-its from the board
        /// </summary>
        public static void Clear()
        {
            Pings.ForEach(Object.Destroy);
            PostIts.ForEach(Object.Destroy);

            Pings.Clear();
            PostIts.Clear();
        }
    }
}