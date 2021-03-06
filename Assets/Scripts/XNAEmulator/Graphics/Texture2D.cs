﻿using System;
using System.IO;
using UnityEngine;

namespace Microsoft.Xna.Framework.Graphics
{
    public class Texture2D : IDisposable
    {
        public int Hash = -1;
        public Texture UnityTexture { get; set; }

        private static byte[] byteArray = new byte[4];

        public Texture2D()
        {
        }

        public Rectangle Bounds => new Rectangle(0, 0, Width, Height);

        public Texture2D(GraphicsDevice graphicsDevice, int width, int height)
        {
            UnityTexture = new UnityEngine.Texture2D(width, height, TextureFormat.ARGB32, false, false);
            UnityTexture.wrapMode = TextureWrapMode.Clamp;
            // UnityTexture.filterMode = FilterMode.Point;
            GraphicDevice = graphicsDevice;
        }

        public GraphicsDevice GraphicDevice { get; set; }

        public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool v, SurfaceFormat surfaceFormat) :
            this(graphicsDevice, width, height)
        {
        }

        public int Width => UnityTexture?.width ?? 0;

        public int Height => UnityTexture?.height ?? 0;

        public bool IsDisposed { get; internal set; }

        public void Dispose()
        {
            if (UnityTexture != null)
            {
                if (UnityTexture is RenderTexture renderTexture)
                {
                    renderTexture.Release();
                }
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying)
                {
                    UnityEngine.Object.Destroy(UnityTexture);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(UnityTexture);
                }
#else
                UnityEngine.Object.Destroy(UnityTexture);
#endif
            }
            UnityTexture = null;
            IsDisposed = true;
        }

        internal void SetData(byte[] data)
        {
            Console.WriteLine("SetData with byte array is not implemented.");
        }

        internal void SetData(ushort[] data)
        {
            SetData(data, data.Length);
        }

        internal void SetData(Color[] data)
        {
            SetData(data, data.Length);
        }

        public static unsafe int u16Tou32(ushort color)
        {
            //Bgra5551
            if (color == 0)
                return 0;
            byte alpha = (byte) ((color >> 15) * 255);
            byte red = (byte) (((color >> 0xA) & 0x1F) * 8.225806f);
            byte green = (byte) (((color >> 0x5) & 0x1F) * 8.225806f);
            byte blue = (byte) ((color & 0x1F) * 8.225806f);
            byteArray[0] = alpha;
            byteArray[1] = red;
            byteArray[2] = green;
            byteArray[3] = blue;

            //NOTE: code below is copied from BitConverter.ToInt32
            fixed (byte* numPtr = &byteArray[0])
            {
                return *(int*) numPtr;
            }
        }

        internal void SetData(ushort[] data, int elementCount)
        {
            var destText = UnityTexture as UnityEngine.Texture2D;
            var dst = destText.GetRawTextureData<uint>();
            var tmp = new uint[elementCount];
            var textureWidth = UnityTexture.width;

            for (int i = 0; i < elementCount; i++)
            {
                int x = i % textureWidth;
                int y = i / textureWidth;
                y *= textureWidth;
                var index = y + (textureWidth - x - 1);
                tmp[i] = (uint) u16Tou32(data[elementCount - index - 1]);
            }

            dst.CopyFrom(tmp);

            destText.Apply();

            Hash = UnityTexture.GetHashCode();
        }

        internal void SetData(uint[] data, int startOffset = 0, int elementCount = 0)
        {
            var textureWidth = UnityTexture.width;

            if (elementCount == 0)
            {
                elementCount = data.Length;
            }
            else
            {
                elementCount *= textureWidth;
            }
            // startOffset *= textureWidth;

            var destText = UnityTexture as UnityEngine.Texture2D;
            var dst = destText.GetRawTextureData<uint>();
            var dstLength = dst.Length;
            var tmp = new uint[dstLength];
            var dataLength = data.Length;

            for (int i = 0; i < elementCount; i++)
            {
                int x = i % textureWidth;
                int y = i / textureWidth;
                y *= textureWidth;
                var index = y + (textureWidth - x - 1);
                if (index < elementCount && i < dstLength)
                {
                    var u = data[dataLength - startOffset - index - 1];
                    uint firstByte = u & 0xff;
                    uint secondByte = (u >> 8) & 0xff;
                    uint thirdByte = (u >> 16) & 0xff;
                    uint fourthByte = (u >> 24) & 0xff;
                    tmp[i] = thirdByte << 24 | secondByte << 16 | firstByte << 8 | fourthByte;
                }
            }

            dst.CopyFrom(tmp);

            destText.Apply();

            Hash = UnityTexture.GetHashCode();
        }

        internal void SetData(Color[] data, int elementCount)
        {
            var destText = UnityTexture as UnityEngine.Texture2D;
            var dst = destText.GetRawTextureData<uint>();
            var tmp = new uint[elementCount];
            var textureWidth = UnityTexture.width;

            for (int i = 0; i < elementCount; i++)
            {
                int x = i % textureWidth;
                int y = i / textureWidth;
                y *= textureWidth;
                var index = y + (textureWidth - x - 1);
                var color = data[elementCount - index - 1];
                //argb -> rgba
                tmp[i] = (uint)(color.R << 24 | color.G << 16 | color.B << 8 | color.A);
            }

            dst.CopyFrom(tmp);

            destText.Apply();

            Hash = UnityTexture.GetHashCode();
        }

        public static void TextureDataFromStreamEXT(
            Stream stream,
            out int width,
            out int height,
            out byte[] pixels,
            int requestedWidth = -1,
            int requestedHeight = -1,
            bool zoom = false)
        {
            width = requestedWidth;
            height = requestedHeight;
            pixels = new[] {(byte)0};
        }

        public override int GetHashCode()
        {
            return Hash;
        }

        public static Texture2D FromStream(GraphicsDevice graphicsDevice, MemoryStream ms)
        {
            //TODO: Implement
            var texture = new Texture2D(graphicsDevice, 2, 2);
            return texture;
        }
    }
}