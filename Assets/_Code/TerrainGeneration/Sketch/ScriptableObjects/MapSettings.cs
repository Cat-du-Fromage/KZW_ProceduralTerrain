using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using Debug = UnityEngine.Debug;


namespace KaizerWaldCode.TerrainGeneration
{
    
    [CreateAssetMenu(menuName = "Map Generation/Map Settings", fileName = "MapSettings")]
    public class MapSettings : ScriptableObject//, ISerializationCallbackReceiver
    {
        [Min(1)]
        [SerializeField] private int chunkSize;
        [Min(1)]
        [SerializeField] private int numChunk;
        [Range(2, 10)]
        [SerializeField] private int pointPerMeter;
        [SerializeField] private string saveFile;

        public string SaveFile { get; private set; }
        public int ChunkSize { get; private set; }
        public int NumChunk { get; private set; }
        public int PointPerMeter { get; private set; }

        public int MapSize { get; private set; }
        public int ChunkPointPerAxis { get; private set; }
        public int MapPointPerAxis { get; private set; }
        public float PointSpacing { get; private set; }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            MapSize = chunkSize * numChunk;
            PointSpacing = 1f / (pointPerMeter - 1f);
            ChunkPointPerAxis = (chunkSize * pointPerMeter) - (chunkSize - 1);
            MapPointPerAxis = (numChunk * chunkSize) * pointPerMeter - (numChunk * chunkSize - 1);
        }
        #endif

        private void OnEnable()
        {
            ChunkSize = max(1, chunkSize);
            NumChunk = max(1, numChunk);
            PointPerMeter = MinMax(2, 10);

            MapSize = chunkSize * numChunk;
            PointSpacing = 1f / (pointPerMeter - 1f);
            ChunkPointPerAxis = (chunkSize * pointPerMeter) - (chunkSize - 1);
            MapPointPerAxis = (numChunk * chunkSize) * pointPerMeter - (numChunk * chunkSize - 1);
            SaveFile = saveFile;
        }

/*
        private void Awake()
        {
            
        }

        public void OnBeforeSerialize()
        {
            throw new NotImplementedException();
        }

        public void OnAfterDeserialize()
        {
            throw new NotImplementedException();
        }
        */
    }
}
