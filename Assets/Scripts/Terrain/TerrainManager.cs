using UnityEngine;
using UnityEngine.Serialization;

namespace Terrain
{
    public class TerrainManager : MonoBehaviour
    {
        public enum MapType
        {
            Shaded,
            Height,
            Slope,
            Normals
        }
        
        public GameObject terrainObject;

        [Header("Terrain")] 
        public float width = 5f;
        public float height = 5f;
        public float maxHeight = .1f;
        public int nx = 20;
        public int ny = 20;
    
        public int seed = 1337;
        public int octaves = 4;
        public float lacunarity = 2f;
        public float gain = .5f;
        public float scale = 1f;
        public MapType mapType;

        [Header("hydraulic erosion")] 
        public int erosionSteps = 100;
        public float soilCapacity = 10f;
        public float minSlope = .05f;
        public float evaporationCoeff = .001f;
        public float erosionSpeed = .9f;
        public float depositionSpeed = .02f;
        public float directionInertia = .1f;
        public float gravity = 20f;
    
        private Terrain terrainGenerator;

        void Awake()
        {
            GenerateTerrain();
            UpdateModel();
        }

        public void GenerateTerrain()
        {
            terrainGenerator = new Terrain(width, height, nx, ny, seed, octaves, lacunarity, gain, scale, maxHeight);
        }

        public void UpdateModel()
        {
            terrainObject.GetComponent<MeshFilter>().sharedMesh = terrainGenerator.GenerateTerrain();
            terrainObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = terrainGenerator.GenerateTexture();
        }

        public void UpdateTexture()
        {
            if(terrainGenerator == null)
                GenerateTerrain();
            
            switch (mapType)
            {
                case MapType.Shaded:
                    terrainObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = terrainGenerator.GenerateTexture();
                    break;
                case MapType.Normals: 
                    terrainObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = terrainGenerator.GenerateNormalsTexture();
                    break;
                case MapType.Height:
                    terrainObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = terrainGenerator.GenerateHeightTexture();
                    break;
                case MapType.Slope:
                    terrainObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = terrainGenerator.GenerateSlopeTexture();
                    break;
                default:
                    break;
            }
        }

        public void Erode()
        {
            terrainGenerator.HydraulicErosion(erosionSteps,
                soilCapacity,
                minSlope,
                evaporationCoeff,
                erosionSpeed,
                depositionSpeed,
                directionInertia,
                gravity);
            UpdateModel();
        }
    }
}
