using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using KaizerWaldCode.TerrainGeneration.Data;
using UnityEngine;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.JsonHelper;
using static KaizerWaldCode.Utils.NativeCollectionUtils;

using dir = KaizerWaldCode.TerrainGeneration.Directories_MapGeneration;

namespace KaizerWaldCode.TerrainGeneration
{
    public class VerticesSliceState : IState
    {
        readonly SettingsData mapSettings;
        readonly int totalChunkPoints;
        readonly int totalMapPoints;
        public ChunksData[] chunksData;
        
        public VerticesSliceState(in SettingsData mapSettings)
        {
            this.mapSettings = mapSettings;
            totalChunkPoints = sq(mapSettings.ChunkPointPerAxis);
            totalMapPoints = sq(mapSettings.MapPointPerAxis);
        }
        public void DoState()
        {
            //GetChunksData(); //Get all chunksDatas
            VerticesSliceProcess();
        }
        /*
        void GetChunksData()
        {
            GameObject[] chunkObjs = GameObject.FindGameObjectsWithTag("Map Chunk");
            chunksData = new ChunksData[chunkObjs.Length];
            for (int i = 0; i < chunkObjs.Length; i++)
            {
                chunksData[i] = chunkObjs[i].GetComponent<ChunksData>();
            }
        }
        */
        void VerticesSliceProcess()
        {
            //Vertices Position
            using NativeArray<float3> verticesPos = AllocNtvAry<float3>(totalChunkPoints);
            //Vertices Cell Index
            using NativeArray<int> verticesCellIndex = AllocNtvAry<int>(totalMapPoints);
            using NativeArray<int> sortedVerticesCellIndex = AllocNtvAry<int>(sq(mapSettings.NumChunk) * totalChunkPoints);
            
            verticesPos.CopyFrom(FromJson<float3>(dir.GetFile_ChunksSharedVertex()));
            verticesCellIndex.CopyFrom(FromJson<int>(dir.GetFile_MapAt(MapFiles.VerticesCellIndex)));
            //Noise Map
            //sortedPerlinNoiseMap = AllocNtvAry<float>(sq(mapSettings.NumChunk) * sq(mapSettings.ChunkPointPerAxis));
            
            DataSliceJob job = new DataSliceJob
            {
                JMapPointPerAxis = mapSettings.MapPointPerAxis,
                JChunkPointPerAxis = mapSettings.ChunkPointPerAxis,
                JNumChunk = mapSettings.NumChunk,
                JVerticesId = verticesCellIndex,
                JSortedVerticesId = sortedVerticesCellIndex,
            };
            JobHandle jobHandle = job.ScheduleParallel(sq(mapSettings.NumChunk), JobsUtility.JobWorkerCount - 1, new JobHandle());
            jobHandle.Complete();

            for (int i = 0; i < chunksData.Length; i++)
            {
                //Use chunk Id since we can't be certain of the order of chunks (oop..)
                int start = chunksData[i].Id * totalChunkPoints;
                ToJson(verticesPos,dir.GetFile_ChunkXYAt(chunksData[i].Position, ChunkFiles.VerticesPos));
                ToJson(sortedVerticesCellIndex.GetSubArray(start, totalChunkPoints),dir.GetFile_ChunkXYAt(chunksData[i].Position, ChunkFiles.VerticesCellIndex));
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        struct DataSliceJob : IJobFor
        {
            [ReadOnly] public int JMapPointPerAxis;
            [ReadOnly] public int JChunkPointPerAxis;
            [ReadOnly] public int JNumChunk;
        
            [ReadOnly] public NativeArray<int> JVerticesId;
        
            [NativeDisableParallelForRestriction]
            [WriteOnly] public NativeArray<int> JSortedVerticesId;
            public void Execute(int index)
            {
                int ChunkPosY = (int)floor(index / (float)JNumChunk);
                int ChunkPosX = index - (ChunkPosY * JNumChunk);

                for (int z = 0; z < JChunkPointPerAxis; z++) // z axis
                {
                    int startY = (ChunkPosY * JMapPointPerAxis) * (JChunkPointPerAxis - 1); //*chunksPoint-1 because of the height of the chunk; -1 because we need the vertice before
                    int startX = ChunkPosX * (JChunkPointPerAxis - 1);
                    int startYChunk = z * JMapPointPerAxis; // y point relative to chunk (NOT CHUNK to MAP)
                    int start = startY + startX + startYChunk;

                    for (int x = 0; x < JChunkPointPerAxis; x++) // x axis
                    {
                        int sliceIndex = mad(z, JChunkPointPerAxis, x) + (index * sq(JChunkPointPerAxis));
                        JSortedVerticesId[sliceIndex] = JVerticesId[start + x];
                    }
                }
            }
        }
    }
}
