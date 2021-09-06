using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEngine;

namespace KaizerWaldCode.TerrainGeneration.Data
{
    [Serializable]
    public struct SettingsData
    {
        public int ChunkSize;
        public int NumChunk;
        public int PointPerMeter;

        public int MapSize;
        public int ChunkPointPerAxis;
        public int MapPointPerAxis;
        public float PointSpacing;

        public static implicit operator SettingsData(MapSettingsData data)
        {
            return new SettingsData()
            {
                ChunkSize = data.ChunkSize,
                NumChunk = data.NumChunk,
                PointPerMeter = data.PointPerMeter,
                MapSize = data.MapSize,
                ChunkPointPerAxis = data.ChunkPointPerAxis,
                MapPointPerAxis = data.MapPointPerAxis,
                PointSpacing = data.PointSpacing,
            };
        }
    }

    public class MapSettingsData
    {
        public int ChunkSize;
        public int NumChunk;
        public int PointPerMeter;

        public int MapSize;
        public int ChunkPointPerAxis;
        public int MapPointPerAxis;
        public float PointSpacing;

        public static implicit operator MapSettingsData(SettingsData data)
        {
            return new MapSettingsData()
            {
                ChunkSize = data.ChunkSize,
                NumChunk = data.NumChunk,
                PointPerMeter = data.PointPerMeter,
                MapSize = data.MapSize,
                ChunkPointPerAxis = data.ChunkPointPerAxis,
                MapPointPerAxis = data.MapPointPerAxis,
                PointSpacing = data.PointSpacing,
            };
        }
    }
}
