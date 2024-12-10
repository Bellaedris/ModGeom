using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terrain
{
    public class Terrain : ScalarField
    {
        // terrain datas
        private ScalarField slopeMap;
        private Vector3[] normalMap;
        //private float[] slope;
        private float maxSlope;

        private float width, height;
        private float maxHeight;
    
        private FastNoiseLite noise;
    
        public Terrain(float width, float height, int nx, int ny, int seed, int octaves, float lacunarity,
            float gain, float scale, float maxHeight)
        : base(nx, ny)
        {
            maxSlope = 0;
            this.width = width;
            this.height = height;
            this.maxHeight = maxHeight;
        
            noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetSeed(seed);
            noise.SetFractalOctaves(octaves);
            noise.SetFractalLacunarity(lacunarity);
            noise.SetFractalGain(gain);
            noise.SetFrequency(0.01f * scale);

            for(int j = 0; j < ny; j++)
            for (int i = 0; i < nx; i++)
            {
                data[j * nx + i] = noise.GetNoise(i, j);
            }

            slopeMap = new ScalarField(nx, ny);
            normalMap = new Vector3[nx * ny];
            ComputeSlopeMap();
            ComputeNormalMap();
        }

        float GetHeight(int x, int y)
        {
            return data[GetIndex(x, y)];
        }

        Vector2 GetGradient(int x, int y)
        {
            float gradX = (data[GetIndex(x + 1, y)] - data[GetIndex(x - 1, y)]) / (2 * width / (float)nx);
            float gradY = (data[GetIndex(x, y + 1)] - data[GetIndex(x, y - 1)]) / (2 * height / (float)ny);
        
            return new Vector2(gradX, gradY);
        }

        float GetSlope(int x, int y)
        {
            return GetGradient(x, y).magnitude;
        }
        
        Vector3 GetNormal(int x, int y)
        {
            Vector2 grad = GetGradient(x, y);
            return new Vector3(-grad.x, 1f, -grad.y).normalized;
        }

        int GetIndex(float x, float y)
        {
            return (int)y * nx + (int)x;
        }

        Vector3 PointInWorld(int index)
        {
            float x = index % nx;
            float z = index / ny;
            return new Vector3(x, data[index], z);
        }

        int WorldToIndex(Vector3 pos)
        {
            return GetIndex(pos.x * (float)nx, pos.z * (float)ny);
        }
    
        public Mesh GenerateTerrain()
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
        
            var vertices = new Vector3[nx * ny];
            var uvs = new Vector2[nx * ny];
            var triangles = new List<int>();
        
            float stepX = width / (nx - 1);
            float stepY = height / (ny - 1);
        
            for(int j = 0; j < ny; j++)
            for (int i = 0; i < nx; i++)
            {
                int index = GetIndex(i, j);
                vertices[index] = new Vector3(i * stepX, data[GetIndex(i, j)] * maxHeight, j * stepY);
                uvs[index] = new Vector2((float)i / (nx), (float)j / (ny));
            }

            for(int j = 0; j < nx - 1; j++)
            for (int i = 0; i < ny - 1; i++)
            {
                int index = (j * nx) + i;
                triangles.Add(index);
                triangles.Add(index + nx);
                triangles.Add(index + nx + 1);
               
                triangles.Add(index);
                triangles.Add(index  + nx + 1);
                triangles.Add(index + 1);
            }
        
            mesh.SetVertices(vertices);
            mesh.uv = uvs;
            mesh.SetTriangles(triangles.ToArray(), 0);
            mesh.RecalculateNormals();

            return mesh;
        }

        public Texture2D GenerateTexture()
        {
            return TextureGenerator.GenerateTexture(ref data, ref slopeMap.data, maxSlope, nx, ny);
        }
        
        public Texture2D GenerateNormalsTexture()
        {
            return TextureGenerator.GenerateNormalTexture(ref normalMap, nx, ny);
        }

        public Texture2D GenerateHeightTexture()
        {
            return TextureGenerator.GenerateGenericGreyscaleTexture(ref data, nx, ny, maxHeight, -maxHeight);
        }
        
        public Texture2D GenerateSlopeTexture()
        {
            return TextureGenerator.GenerateGenericGreyscaleTexture(ref data, nx, ny, maxSlope);
        }

        private void ComputeSlopeMap()
        {
            float[] slopes = new float[nx * ny];
            for(int j = 1; j < ny - 1; j++)
            for (int i = 1; i < nx - 1; i++)
            {
                float slope = GetSlope(i, j);
                slopes[GetIndex(i, j)] = slope;
                if (slope > maxSlope)
                    maxSlope = slope;
            }
            slopeMap.SetData(slopes);
        }

        private void ComputeNormalMap()
        {
            for(int j = 1; j < ny - 1; j++)
            for (int i = 1; i < nx - 1; i++)
            {
                Vector3 normal = GetNormal(i, j);
                normalMap[GetIndex(i, j)] = normal;
            }
        }
    }
}