using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.TerrainGeneration.Data
{
    [Serializable]
    public struct SettingsData
    {
        //WRONG REPLACE RADIUS BY CELLSIZE AND VICE VERSA
        public int ChunkSize;
        public int NumChunk;
        public int PointPerMeter;
        public int Radius;
        public uint Seed;

        public int MapSize;
        public int ChunkPointPerAxis;
        public int MapPointPerAxis;
        public int NumCellMap;
        public float PointSpacing;
        public float CellSize;

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
                Radius = data.Radius,
                Seed = data.Seed,
                NumCellMap = data.NumCellMap,
                CellSize = data.CellSize,
            };
        }
    }

    public class MapSettingsData
    {
        public int ChunkSize;
        public int NumChunk;
        public int PointPerMeter;
        public int Radius;
        public uint Seed;

        public int MapSize;
        public int ChunkPointPerAxis;
        public int MapPointPerAxis;
        public int NumCellMap;
        public float PointSpacing;
        public float CellSize;

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
                Radius = data.Radius,
                Seed = data.Seed,
                NumCellMap = data.NumCellMap,
                CellSize = data.CellSize,
            };
        }
    }
}
