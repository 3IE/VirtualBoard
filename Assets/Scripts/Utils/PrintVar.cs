using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Utils
{
    public class PrintVar : MonoBehaviour
    {
        private static TMP_Text _text;
        private static Dictionary<uint, string> _lines = new();
        private static StringBuilder _textToPrint = new();
    
        private static uint _lineId = 0;

        private void Awake() 
            => _text = GetComponent<TMP_Text>();

        /// <summary>
        /// Prints on a specific line
        /// </summary>
        /// <param name="n"> Index of the line </param>
        /// <param name="args"> Content to print </param>
        public static void Print(uint n, params string[] args)
        {
            _text.faceColor = Color.black;

            _lineId = Math.Max(_lineId, n);
            _lines[n] = string.Join("\n", args);
            _textToPrint.Clear();
            _textToPrint.AppendJoin("\n\n", _lines.Values);
            _text.text = _textToPrint.ToString();
        }

        /// <summary>
        /// Prints on the next line available
        /// </summary>
        /// <param name="args"> Content to print </param>
        public static void Print(params string[] args)
        {
            _text.faceColor = Color.black;

            _lines[++_lineId] = string.Join("\n", args);
            _textToPrint.AppendJoin("\n\n", _lines.Values);
            _text.text = _textToPrint.ToString();
        }

#if UNITY_EDITOR

        /// <summary>
        /// Prints on a specific line
        /// </summary>
        /// <param name="n"> Index of the line </param>
        /// <param name="args"> Content to print </param>
        public static void PrintDebug(uint n, params string[] args)
        {
            _text.faceColor = Color.red;

            _lineId = Math.Max(_lineId, n);
            _lines[n] = string.Join("\n", args);
            _textToPrint.Clear();
            _textToPrint.AppendJoin("\n\n", _lines.Values);
            _text.text = _textToPrint.ToString();
        }

        /// <summary>
        /// Prints on the next line available
        /// </summary>
        /// <param name="args"> Content to print </param>
        public static void PrintDebug(params string[] args)
        {
            _text.faceColor = Color.red;

            _lines[++_lineId] = string.Join("\n", args);
            _textToPrint.AppendJoin("\n\n", _lines.Values);
            _text.text = _textToPrint.ToString();
        }

#endif

        /// <summary>
        /// Resets the displayed content
        /// </summary>
        public static void Clear() {
            _lines.Clear();
            _textToPrint.Clear();
            _text.text = "";
        }
    }
}