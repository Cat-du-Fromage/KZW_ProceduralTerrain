using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using KaizerWaldCode.KWSerialization;
using KaizerWaldCode.Utils;
using KaizerWaldCode.TerrainGeneration.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.JsonHelper;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using Debug = UnityEngine.Debug;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    /// <summary>
    /// VERTICES Slice
    /// </summary>
    public partial class MapSystem : MonoBehaviour
    {
        private void VerticesSliceProcess()
        {
            int chunkPoints = mapSettings.ChunkPointPerAxis;
            int mapPoints = mapSettings.MapPointPerAxis;
            //Vertices Position
            verticesPos = AllocNtvAry<float3>(sq(chunkPoints));
            verticesPos.CopyFrom(FromJson<float3>(dir.GetChunksSharedVertexFile()));
            
            //Vertices Cell Index
            verticesCellIndex = AllocNtvAry<int>(sq(mapPoints));
            verticesCellIndex.CopyFrom(FromJson<int>(dir.GetFullMapFileAt((int) MapFiles.VerticesCellIndex)));
            sortedVerticesCellIndex = AllocNtvAry<int>(sq(mapSettings.NumChunk) * sq(chunkPoints));
            
            //Noise Map
            //sortedPerlinNoiseMap = AllocNtvAry<float>(sq(mapSettings.NumChunk) * sq(mapSettings.ChunkPointPerAxis));
            
            DataSliceJobProcess();

            for (int i = 0; i < chunks.Length; i++)
            {
                //Use chunk Id since we can't be certain of the order of chunks (oop..)
                int start = chunks[i].Id * sq(mapSettings.ChunkPointPerAxis);
                ToJson(verticesPos,dir.GetChunkFileAt(chunks[i].Position, (int)ChunkFiles.VerticesPos));
                ToJson(sortedVerticesCellIndex.GetSubArray(start, sq(chunkPoints)),dir.GetChunkFileAt(chunks[i].Position, (int)ChunkFiles.VerticesCellIndex));
            }

            verticesPos.Dispose();
            verticesCellIndex.Dispose();
            sortedVerticesCellIndex.Dispose();
            //sortedPerlinNoiseMap.Dispose();
        }
        private void DataSliceJobProcess(in JobHandle dependency = new JobHandle())
        {
            DataSliceJob dataSliceJ = new DataSliceJob
            {
                JMapPointPerAxis = mapSettings.MapPointPerAxis,
                JChunkPointPerAxis = mapSettings.ChunkPointPerAxis,
                JNumChunk = mapSettings.NumChunk,
                JVerticesId = verticesCellIndex,
                JSortedVerticesId = sortedVerticesCellIndex,
            };
            JobHandle dataSliceJH = dataSliceJ.ScheduleParallel(sq(mapSettings.NumChunk), JobsUtility.JobWorkerCount - 1, dependency);
            dataSliceJH.Complete();
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct DataSliceJob : IJobFor
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
