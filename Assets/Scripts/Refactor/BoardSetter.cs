using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Refactor
{
    public class BoardSetter : MonoBehaviour
    {
        private const int PIXEL_THRESHOLD = 2796202;

        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private GameObject    boardSetter;

        private Texture2D _texture;

        // Start is called before the first frame update
        private void Start()
        {
            _texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32,
                                     false);
            boardSetter.GetComponent<Renderer>().material.mainTexture = _texture;
        }

        public void SetTexture(object[] data)
        {
            _texture.LoadImage(data[0] as byte[]);
            _texture.Apply();

            /*
            var full = (bool) data[0];

            if (full)
                SetTexture(data[1] as float[], data[2] as float[]);
            else
                SetPartialTexture(data[1] as int[], data[2] as float[], data[3] as float[]);
            */

            boardSetter.SetActive(true);

            //Invoke(nameof(DeactivateBoardSetter), 0.1f);
        }

        private void SetPartialTexture(int[] indexes, float[] colors, float[] alphas)
        {
            Color[] pixels = _texture.GetPixels();

            for (var i = 0; i < indexes.Length; i++)
            {
                int     index             = indexes[i];
                Vector3 decompressedColor = Compression.UnpackVector3(colors[i], Vector3.zero, Vector3.one);

                var color = new Color(decompressedColor.x, decompressedColor.y, decompressedColor.z,
                                      alphas[i]);
                pixels[index] = color;
            }

            _texture.SetPixels(pixels);
            _texture.Apply();
        }

        private void SetTexture(float[] colors, float[] alphas)
        {
            var pixels = new Color[colors.Length];

            for (var i = 0; i < colors.Length; i++)
            {
                Vector3 decompressedColor = Compression.UnpackVector3(colors[i], Vector3.zero, Vector3.one);

                var color = new Color(decompressedColor.x, decompressedColor.y, decompressedColor.z,
                                      alphas[i]);
                pixels[i] = color;
            }

            _texture.SetPixels(pixels);
            _texture.Apply();
        }

        private void DeactivateBoardSetter()
        {
            boardSetter.SetActive(false);
        }

        public object[] GetTexture()
        {
            return new object[] { BoardCapture.RenderToTexture(renderTexture, true).EncodeToPNG() };

            /*
            Texture2D texture = BoardCapture
                .RenderToTexture(renderTexture);

            Color[] colors = texture.GetPixels();

            List<(int i, Vector3 vec, float a)> pixels       = new();
            var                                 filterNumber = 0;

            for (var i = 0; i < colors.Length; i++)
            {
                Color color = colors[i];

                if (1 - color.a >= 0.01)
                    continue;

                pixels.Add((i, new Vector3(color.r, color.g, color.b), color.a));
                filterNumber++;
            }

            if (filterNumber == 0)
                return null;

            bool sendFull = filterNumber > PIXEL_THRESHOLD;

            float[] colorsCompressed;

            float[] alphas;

            if (!sendFull)
            {
                int[] indexes = pixels.Select(p => p.i).ToArray();

                colorsCompressed = pixels.Select(p => Compression.PackVector3(p.vec, Vector3.zero, Vector3.one))
                                         .ToArray();
                alphas = pixels.Select(p => p.a).ToArray();

                return new object[]
                {
                    false,
                    indexes,
                    colorsCompressed,
                    alphas,
                };
            }

            colorsCompressed = pixels.Select(p => Compression.PackVector3(p.vec, Vector3.zero, Vector3.one)).ToArray();
            alphas           = pixels.Select(p => p.a).ToArray();

            return new object[]
            {
                true,
                colorsCompressed,
                alphas,
            };
            */
        }
    }
}