using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Terrain
{
    public class Terrain : ScalarField
    {
        struct HeightGradient
        {
            public float height;
            public float gradientX;
            public float gradientY;

            public HeightGradient(float height, float gradientX, float gradientY)
            {
                this.height = height;
                this.gradientX = gradientX;
                this.gradientY = gradientY;
            }
        }
        
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
            float gradX = (data[GetIndex(x + 1, y)] - data[GetIndex(x - 1, y)]) / (2 * width / nx);
            float gradY = (data[GetIndex(x, y + 1)] - data[GetIndex(x, y - 1)]) / (2 * height / ny);
        
            return new Vector2(gradX, gradY);
        }

        HeightGradient GetBilinearHeightGradient(float px, float py)
        {
            int coordX = (int)px;
            int coordY = (int)py;

            // local position inside the cell
            float x = px - coordX;
            float y = py - coordY;
            
            // height at the 4 neighbors of the cell
            float heightNW = GetHeight(coordX, coordY);
            float heightNE = GetHeight(coordX + 1, coordY);
            float heightSW = GetHeight(coordX, coordY + 1);
            float heightSE = GetHeight(coordX + 1, coordY + 1);
            
            //bilinear gradient, aka weighted finite difference
            float gradX = (heightNE - heightNW) * (1 - y) + (heightSE - heightSW) * y;
            float gradY = (heightSW - heightNW) * (1 - x) + (heightSE - heightNE) * x;
            
            // bilinear height
            float height = heightNW * (1 - x) * (1 - y) + heightNE * x * (1 - y) + heightSW * (1 - x) * y +
                           heightSE * x * y;
            
            return new HeightGradient(height, gradX, gradY);
        }

        void BilinearAdd(int index, float weightX, float weightY, float amount)
        {
            data[index] += amount * (1 - weightX) * (1 - weightY);
            data[index + 1] += amount * weightX * (1 - weightY);
            data[index + nx] += amount * (1 - weightX) * weightY;
            data[index + nx + 1] += amount * weightX * weightY;
        }
        
        void Erode(float x, float y, float amount)
        {
            int xi = (int)x;
            int yi = (int)y;

            for (int offsetY = -1; offsetY <= 2; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 2; offsetX++)
                {
                    float dx = offsetX - (x - xi);
                    float dy = offsetY - (y - yi);
                    float weight = Mathf.Max(0, 1 - (dx * dx + dy * dy) * 0.25f) * 0.159154943f;
                    data[GetIndex(xi + offsetX, yi + offsetY)] -= amount * weight;
                }
            }
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

        int WorldToIndex(float x, float y)
        {
            return GetIndex(x / width * nx, y / height * ny);
        }

        Vector3 IndexToWorld(int index)
        {
            int x = index % nx; // Column
            int z = index / nx; // Row

            return new Vector3((float)x / nx * width, data[index], (float)z / ny * height);
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

        public void HydraulicErosion(int iterations,
            float soilCapacity,
            float minSlope,
            float evaporationCoeff,
            float erosionSpeed,
            float depositionSpeed,
            float directionInertia,
            float gravity)
        {
            int maxDropletMovements = 100;
            for (int i = 0; i < iterations; i++)
            {
                //pick random coords to start the algorithm
                float posX = Random.Range(1f, nx - 2f); 
                float posY = Random.Range(1f, ny - 2f);
                float dirX = 0;
                float dirY = 0;
                float sediment = 0, velocity = 1, waterAmount = 1; // sediments, flow speed, amount of water
                
                for (int j = 0; j < maxDropletMovements; j++)
                {
                    int px = (int)posX;
                    int py = (int)(posY);
                    float offsetX = posX - px;
                    float offsetY = posY - py;

                    var heightGrad = GetBilinearHeightGradient(posX, posY);
                    
                    // compute gradient
                    dirX = dirX * directionInertia - heightGrad.gradientX * (1 - directionInertia);
                    dirY = dirY * directionInertia - heightGrad.gradientY * (1 - directionInertia);
                    
                    float length = Mathf.Sqrt(dirX * dirX + dirY * dirY);
                    // compute direction of next cell
                    if (length < 0.001f)
                    {
                        // if slope is too low, pick a random dir
                        float angle = Random.Range(0f, Mathf.PI * 2f);
                        dirX = Mathf.Cos(angle);
                        dirY = Mathf.Sin(angle);
                    }
                    else
                    {
                        // else direction is the normalized gradient
                        dirX /= length;
                        dirY /= length;
                    }
                    
                    posX += dirX;
                    posY += dirY;
                    
                    if (posX < 4 || posX >= nx - 5 || posY < 4 || posY >= ny - 5)
                        break;
                    
                    // sample height at cell at direction
                    int newX = (int)posX;
                    int newY = (int)posY;
                    float newH = GetBilinearHeightGradient(posX, posY).height;
                    float deltaH = newH - heightGrad.height;

                    // erode if current is lower than new
                    if (deltaH > 0f)
                    {
                        float toDeposit = Mathf.Min(sediment, deltaH);
                        sediment -= toDeposit;
                        BilinearAdd(GetIndex(px, py), offsetX, offsetY, toDeposit);
                    }

                    // q the transport capacity = max(diffHeight, minSlope) * v * w * Kq (constant for soil carry capacity)
                    float sedimentCapacity = Mathf.Max(-deltaH, minSlope) * velocity * waterAmount * soilCapacity;
                    float sDiff = sediment - sedimentCapacity;
                    // if there is more sediments than the capacity, drop the difference
                    if (sDiff > 0f)
                    {
                        float toDeposit = sDiff * depositionSpeed;
                        BilinearAdd(GetIndex(px, py), offsetX, offsetY, toDeposit);
                        sediment -= toDeposit;
                    }
                    else
                    {
                        float toErode = Mathf.Min((sedimentCapacity - sediment) * erosionSpeed, -deltaH);
                        //BilinearAdd(GetIndex(px, py), offsetX, offsetY, -toErode);
                        Erode(px, py, toErode);
                        
                        sediment += toErode;
                    }
                    // if the droplet has more sediments than the max transport capacity, deposit the difference
                    // else, erode to add sediments to the droplet and remove height from the terrain

                    velocity = Mathf.Sqrt(Mathf.Max(velocity * velocity + gravity * deltaH, 0f));
                    waterAmount *= (1 - evaporationCoeff); // evaporation rate
                    //Debug.DrawLine(IndexToWorld(GetIndex(px, py)), IndexToWorld(GetIndex(newX, newY)), Color.red, 20f, false);
                    //Debug.Log($"posX: {posX}, posY: {posY}, Sediment: {sediment}, Velocity: {velocity}");
                }
            }

            UpdateMaps();
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
            return TextureGenerator.GenerateGenericGreyscaleTexture(ref slopeMap.data, nx, ny, maxSlope);
        }

        void UpdateMaps()
        {
            ComputeSlopeMap();
            ComputeNormalMap();
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