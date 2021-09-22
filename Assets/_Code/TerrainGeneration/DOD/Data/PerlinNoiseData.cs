using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWaldCode.TerrainGeneration.Data
{
    [Serializable]
    public struct PerlinNoiseData
    {
        public uint Seed;
        public int Octaves;
        public float Scale;
        public float Persistence;
        public float Lacunarity;
        public float HeightMultiplier;
        public float2 Offset;
    }
}
