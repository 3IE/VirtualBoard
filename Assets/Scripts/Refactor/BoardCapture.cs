using System;
using System.IO;
using UnityEngine;

namespace Refactor
{
    public class BoardCapture : MonoBehaviour
    {
        [SerializeField] private RenderTexture renderTexture;

        #if UNITY_EDITOR

        public bool test;

        private void Update()
        {
            if (!test)
                return;

            test = false;

            SaveTextureToDisk();
        }

        #endif

        public void SaveTextureToDisk()
        {
            Texture2D texture = RenderToTexture(renderTexture);
            byte[]    bytes   = texture.EncodeToPNG();

            var path     = $"{Application.persistentDataPath}/";
            var fileName = $"{path}board_{DateTime.Now:M_d_yy_h_mm_ss}.png";

            var file = new FileInfo(fileName);
            file.Directory?.Create();
            File.WriteAllBytes(file.FullName, bytes);

            Debug.Log($"Saved texture to {file.FullName}");

            Destroy(texture);
        }

        public static Texture2D RenderToTexture(RenderTexture render, bool toSend = false)
        {
            var texture = new Texture2D(render.width, render.height,
                                        toSend ? TextureFormat.ARGB32 : TextureFormat.RGBA32,
                                        false);

            //Graphics.CopyTexture(render, texture);

            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = render;

            texture.ReadPixels(new Rect(0, 0, render.width,
                                        render.height), 0, 0);
            texture.Apply();

            RenderTexture.active = rt;

            return texture;
        }
    }
}