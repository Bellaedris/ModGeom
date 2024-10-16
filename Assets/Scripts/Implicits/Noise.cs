using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace vxl
{
    public class Noise : Node
    {
        private FastNoiseLite _noise;
        private float _maxHeight;
        
        private Vector3 _normal;
        private float _height;

        public Noise(Vector3 normal, float height, int seed, int octaves, float scale, float gain, float lacunarity, float maxHeight)
        {
            _noise = new FastNoiseLite();
            _normal = normal.normalized;
            _height = height;
            _maxHeight = maxHeight;
            
            _bounds = new Bounds(Vector3.zero + _normal * _height, Vector3.one);
            
            _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            _noise.SetSeed(seed);
            _noise.SetFractalOctaves(octaves);
            _noise.SetFrequency(scale);
            _noise.SetFractalGain(gain);
            _noise.SetFractalLacunarity(lacunarity);
        }
        
        public float Distance(Vector3 point)
        {
            return Vector3.Dot(point, _normal) + _height + _noise.GetNoise(point.x, point.z) * _maxHeight;
        }
        
        public Color GetColor()
        {
            
        }
    }
}