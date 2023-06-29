using System;
using UnityEngine;
using System.Collections;

namespace OrkestraLib
{
    namespace Utilities
    {
        public static class ImageExtensions
        {
            public static string ToBase64(this byte[] bytesArr)
            {
                return "data:image/png;base64," + Convert.ToBase64String(bytesArr);
            }

            public static byte[] Capture(this Camera camera, Rect rect)
            {
                RenderTexture rt = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0);
                camera.targetTexture = rt;
                camera.Render();

                RenderTexture.active = rt;
                Texture2D screenShot = new Texture2D(camera.pixelWidth, camera.pixelHeight, TextureFormat.RGBA32, false);

                screenShot.ReadPixels(rect, 0, 0);
                screenShot.Apply();

                camera.targetTexture = null;
                RenderTexture.active = null;
                GameObject.Destroy(rt);

                byte[] bytes = screenShot.EncodeToPNG();
                return bytes;
            }

            public static IEnumerator ScreenshotEncode()
            {
                // wait for graphics to render
                yield return new WaitForEndOfFrame();
                string text = Camera.main.Capture(Screen.safeArea).ToBase64();
            }
        }
    }
}