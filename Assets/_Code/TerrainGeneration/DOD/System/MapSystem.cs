using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KaizerWaldCode.TerrainGeneration.Data;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.KWSerialization.BinarySerialization;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    public partial class MapSystem : MonoBehaviour
    {

        private SettingsData mapSettings;
        private PerlinNoiseData noiseSettings;
        
        private int mapPointSurface;

        private BitField32 bitfield;

        //private GameObject[] chunks;
        private ChunksData[] chunks;

        private NativeArray<float3> verticesPos; // raw position calcul
        private NativeArray<int> verticesCellIndex; // raw position calcul

        private NativeArray<float3> sortedVerticesPos; // use for chunkSlice
        private NativeArray<int> sortedVerticesCellIndex; // use for chunkSlice
        
        private NativeArray<int> triangles;
        private NativeArray<int> chunkTriangles;
        private NativeArray<float2> uvs;
        private NativeArray<float2> sortedUvs;
        
        //NOISE
        private NativeArray<float2> noiseOffsetsMap;
        private NativeArray<float> perlinNoiseMap;
        private NativeArray<float> sortedPerlinNoiseMap;

        private JobHandle gDependency; // needed for jobs system

        public void LoadMap(in SettingsData mapSet, in PerlinNoiseData noiseSet , in bool newGame, in string folderName = "default")
        {
            bitfield = new BitField32(uint.MaxValue);
            mapSettings = mapSet;
            noiseSettings = noiseSet;
            mapPointSurface = sq(mapSet.MapPointPerAxis);

            dir.SelectedSave = folderName;
            
            if (newGame)
            {
                //Destroy any existing save with the same name
                if (Directory.Exists(dir.SelectSavePath))
                {
                    Directory.Delete(dir.SelectSavePath, true);
                }
                GetOrCreateDirectories(folderName);
                LoadNewMap();
            }
            else
            {
                LoadSavedMap();
            }

        }

        public void LoadSavedMap()
        {
        }

        public void LoadNewMap()
        {
            VerticesPositionProcess();
            SharedVerticesPositionProcess();
            VerticesCellIndexProcess();
            PoissonDiscProcess();
            IslandCoastProcess();
            //VoronoiIsland();

            CreateChunkProcess();
            
            PerlinNoiseProcess(); // TO DO : Seperate slicing process
            
            VerticesSliceProcess();
            MeshDatasProcess();
            BuildMeshesProcess();
        }
        
        void OnDestroy()
        {
            if (verticesPos.IsCreated) verticesPos.Dispose();
            if (verticesCellIndex.IsCreated) verticesCellIndex.Dispose();
            if (sortedVerticesPos.IsCreated) sortedVerticesPos.Dispose();
            if (sortedVerticesCellIndex.IsCreated) sortedVerticesCellIndex.Dispose();
            if (triangles.IsCreated) triangles.Dispose();
            if (uvs.IsCreated) uvs.Dispose();
            if (sortedUvs.IsCreated) sortedUvs.Dispose();
            if (chunkTriangles.IsCreated) chunkTriangles.Dispose();
            if (noiseOffsetsMap.IsCreated) noiseOffsetsMap.Dispose();
            if (perlinNoiseMap.IsCreated) perlinNoiseMap.Dispose();
            if (sortedPerlinNoiseMap.IsCreated) sortedPerlinNoiseMap.Dispose();
        }
    }
}
