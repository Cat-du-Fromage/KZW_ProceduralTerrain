using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.KwDelaunay;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.KWSerialization.BinarySerialization;

using dir2 = KaizerWaldCode.TerrainGeneration.Directories_MapGeneration;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    public partial class MapSystem : MonoBehaviour
    {

        SettingsData mapSettings;
        PerlinNoiseData noiseSettings;
        
        int mapPointSurface;

        BitField32 bitfield;

        //private GameObject[] chunks;
        ChunksData[] chunks;

        NativeArray<float3> verticesPos; // raw position calcul
        NativeArray<int> verticesCellIndex; // raw position calcul

        NativeArray<float3> sortedVerticesPos; // use for chunkSlice
        NativeArray<int> sortedVerticesCellIndex; // use for chunkSlice
        
        NativeArray<int> triangles;
        NativeArray<int> chunkTriangles;
        NativeArray<float2> uvs;
        NativeArray<float2> sortedUvs;
        
        //NOISE
        NativeArray<float2> noiseOffsetsMap;
        NativeArray<float> perlinNoiseMap;
        NativeArray<float> sortedPerlinNoiseMap;

        private JobHandle gDependency; // needed for jobs system

        public void LoadMap(in SettingsData mapSet, in PerlinNoiseData noiseSet , in bool newGame, in string folderName = "default")
        {
            Directories_MapGeneration testDir = new Directories_MapGeneration(in folderName);
            Debug.Log(Directories_MapGeneration.ToString());
            bitfield = new BitField32(uint.MaxValue);
            mapSettings = mapSet;
            noiseSettings = noiseSet;
            mapPointSurface = sq(mapSet.MapPointPerAxis);

            //dir = new MapDirectories(folderName);
            //dir.SelectedSave = folderName;
            
            if (newGame)
            {
                //Destroy any existing save with the same name
                if (Directory.Exists(dir2.GetFolder_SelectedSave))
                {
                    Directory.Delete(dir2.GetFolder_SelectedSave, true);
                }
                
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
            TerrainGenerationDirectoriesTree testTree = new TerrainGenerationDirectoriesTree(in mapSettings);
            MapDatasStateMachine testState = new MapDatasStateMachine(in mapSettings);
            ChunkSlicerStateMachine testSlice = new ChunkSlicerStateMachine(in mapSettings);
            IslandStateMachine testIsland = new IslandStateMachine(in mapSettings);
            /*
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
            DelaunaySystem tstDel = new DelaunaySystem(dir.SelectedSave, mapSettings);
            */
            //VoronoiProcess.VoronoiMap(dir, mapSettings);
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
