using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Board.Events
{
    public static class PlayerEvents
    {
        private static GameObject _postItPrefab;
        private static GameObject _onlinePingPrefab;
        private static GameObject _board;

        public static readonly List<GameObject> Pings = new();
        public static readonly List<GameObject> PostIts = new();

        public static void SetupPrefabs(GameObject postItPrefab, GameObject onlinePingPrefab, GameObject board)
        {
            _postItPrefab = postItPrefab;
            _onlinePingPrefab = onlinePingPrefab;
            _board = board;
        }

        public static void ReceiveNewPostIt(object[] data)
        {
            var position = (Vector2)data[0];
            var text = (string)data[1];
            var postIt = Object.Instantiate(_postItPrefab, _board.transform);
            var invertScale = 1 / _board.transform.localScale.x;
            
            postIt.transform.localPosition = new Vector3(position.x * invertScale, 0.01f, position.y * invertScale);
            postIt.transform.rotation = Quaternion.identity;
            postIt.transform.localScale *= invertScale;

            postIt.GetComponentInChildren<TMP_Text>().text = text;
            // TODO: change color depending on the player who created the post it
            postIt.GetComponentInChildren<Renderer>().material.color = Color.cyan;

            PostIts.Add(postIt);
            
            // print($"Post-it: {text}, at {position}");
        }

        public static void ReceivePing(Vector2 position)
        {
            var ping = Object.Instantiate(_onlinePingPrefab, _board.transform);
            var invertScale = 1 / _board.transform.localScale.x;
            ping.transform.localPosition = new Vector3(position.x * invertScale, 0.01f, position.y * invertScale);
            ping.transform.rotation = Quaternion.identity;
            ping.transform.localScale *= invertScale;

            Pings.Add(ping);
            
            /*
             TODO: add PingSearcher
             _currentPing = ping.transform.position;
             _pingSearcher.AssignedPing = ping;
             _pingSearcher.gameObject.SetActive(true);
            */
        }

        public static void Clear()
        {
            Pings.ForEach(Object.Destroy);
            PostIts.ForEach(Object.Destroy);
        
            Pings.Clear();
            PostIts.Clear();
        }
    }
}