using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    public class TextureGenerator
    {
        public static Texture2D GenerateTexture(ref float[] hm, ref float[] slope, float maxSlope, int nx, int ny)
        {
            var texture = new Texture2D(nx, ny);
            Color[] pixels = new Color[nx * ny];

            for(int j = 0; j < ny; j++)
            for (int i = 0; i < nx; i++)
            {
                int index = j * nx + i;
                if (hm[index] < -0.5f)
                    pixels[index] = Color.blue;
                else if (hm[index] < -0.45f)
                    pixels[index] = Color.yellow;
                else if (hm[index] < 0.5f)
                    pixels[index] = Color.Lerp(Color.green, new Color(.4f, .254f, 0.023f), slope[index] / maxSlope);
                else
                {
                    if (slope[index] / maxSlope > .5f)
                        pixels[index] = Color.gray;
                    else
                        pixels[index] = Color.white;
                }
                    
            }

            texture.SetPixels(pixels, 0);
            texture.filterMode = FilterMode.Trilinear;
            texture.Apply();
            return texture;
        }

        public static Texture2D GenerateGenericGreyscaleTexture(ref float[] data, int nx, int ny, float maxValue,
            float minValue = 0f)
        {
            var texture = new Texture2D(nx, ny);
            Color[] pixels = new Color[nx * ny];
            
            for(int j = 0; j < ny; j++)
            for (int i = 0; i < nx; i++)
            {
                int index = j * nx + i;
                float col = Utils.Normalize01(data[index], minValue, maxValue);
                pixels[index] = new Color(col, col, col);
            }
            
            texture.SetPixels(pixels, 0);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return texture;
        }

        public static Texture2D GenerateNormalTexture(ref Vector3[] normals, int nx, int ny)
        {
            var texture = new Texture2D(nx, ny);
            Color[] pixels = new Color[nx * ny];
            for(int j = 0; j < ny; j++)
            for (int i = 0; i < nx; i++)
            {
                int index = j * nx + i;
                var normalized = (normals[index] + Vector3.one) * .5f;
                pixels[index] = new Color(normalized.x, normalized.y, normalized.z);
            }

            texture.SetPixels(pixels, 0);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return texture;
        }
    }
}
