﻿using TMPro;
using UnityEngine;

namespace Board.Events
{
    public static class PlayerEvents
    {
        private static GameObject _postItPrefab;
        private static GameObject _onlinePingPrefab;
        private static GameObject _boardTransform;

        public static void SetupPrefabs(GameObject postItPrefab, GameObject onlinePingPrefab, GameObject boardTransform)
        {
            _postItPrefab = postItPrefab;
            _onlinePingPrefab = onlinePingPrefab;
            _boardTransform = boardTransform;
        }

        public static void ReceiveNewPostIt(object[] data)
        {
            var postItPos = (Vector2)data[0];
            var text = (string)data[1];
            var boardPosPostIt = _boardTransform.transform.position;
            
            var position = new Vector3(boardPosPostIt.x + postItPos.x,
                boardPosPostIt.y + postItPos.y, boardPosPostIt.z);
            var postIt = Object.Instantiate(_postItPrefab, position, _boardTransform.transform.rotation,
                _boardTransform.transform);

            postIt.GetComponentInChildren<TMP_Text>().text = text;
            // TODO: change color depending on the player who created the post it
            postIt.GetComponentInChildren<Renderer>().material.color = Color.cyan;

            // print($"Post-it: {text}, at {position}");
        }

        public static void ReceivePing(Vector2 position)
        {
            var ping = Object.Instantiate(_onlinePingPrefab, new Vector3(0, 0, 0), _boardTransform.transform.rotation,
                _boardTransform.transform);
            ping.transform.localPosition = new Vector3(position.x, position.y, 0);

            /*
             TODO: add PingSearcher
             _currentPing = ping.transform.position;
             _pingSearcher.AssignedPing = ping;
             _pingSearcher.gameObject.SetActive(true);
            */
        }
    }
}