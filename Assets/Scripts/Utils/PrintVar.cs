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
        private static readonly Dictionary<uint, string> Lines = new();
        private static readonly StringBuilder TextToPrint = new();
    
        private static uint _lineId;

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
            Lines[n] = string.Join("\n", args);
            TextToPrint.Clear();
            TextToPrint.AppendJoin("\n\n", Lines.Values);
            _text.text = TextToPrint.ToString();
        }

        /// <summary>
        /// Prints on the next line available
        /// </summary>
        /// <param name="args"> Content to print </param>
        public static void Print(params string[] args)
        {
            _text.faceColor = Color.black;

            Lines[++_lineId] = string.Join("\n", args);
            TextToPrint.AppendJoin("\n\n", Lines.Values);
            _text.text = TextToPrint.ToString();
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
            Lines[n] = string.Join("\n", args);
            TextToPrint.Clear();
            TextToPrint.AppendJoin("\n\n", Lines.Values);
            _text.text = TextToPrint.ToString();
        }

        /// <summary>
        /// Prints on the next line available
        /// </summary>
        /// <param name="args"> Content to print </param>
        public static void PrintDebug(params string[] args)
        {
            _text.faceColor = Color.red;

            Lines[++_lineId] = string.Join("\n", args);
            TextToPrint.AppendJoin("\n\n", Lines.Values);
            _text.text = TextToPrint.ToString();
        }

#endif

        /// <summary>
        /// Resets the displayed content
        /// </summary>
        public static void Clear() {
            Lines.Clear();
            TextToPrint.Clear();
            _text.text = "";
        }
    }
}