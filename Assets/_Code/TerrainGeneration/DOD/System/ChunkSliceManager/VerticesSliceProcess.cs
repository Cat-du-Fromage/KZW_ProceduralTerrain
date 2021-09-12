using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.TerrainGeneration.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.KWSerialization.BinarySerialization;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    /// <summary>
    /// VERTICES Slice
    /// </summary>
    public partial class MapSystem : MonoBehaviour
    {
        private void VerticesSliceProcess()
        {
            verticesPos = AllocNtvAry<float3>(sq(mapSettings.MapPointPerAxis));
            verticesPos.CopyFrom(Load<float3>(paths[0]));

            sortedVerticesPos = AllocNtvAry<float3>(sq(mapSettings.NumChunk) * sq(mapSettings.ChunkPointPerAxis));
            VerticesSliceJobProcess(gDependency);
            for (int i = 0; i < chunks.Length; i++)
            {
                ChunksData chunkData = chunks[i].GetComponent<ChunksData>();
                //Use chunk Id since we can't be certain of the order of chunks (oop..)
                int start = chunkData.Id * sq(mapSettings.ChunkPointPerAxis);
                sortedVerticesPos.GetSubArray(start, sq(mapSettings.ChunkPointPerAxis)).CopyTo(chunkData.Vertices);
            }

            verticesPos.Dispose();
            sortedVerticesPos.Dispose();
        }

        private void VerticesSliceJobProcess(in JobHandle dependencySystem)
        {
            VerticesSliceJob verticesSliceJob = new VerticesSliceJob
            {
                JMapPointPerAxis = mapSettings.MapPointPerAxis,
                JChunkPointPerAxis = mapSettings.ChunkPointPerAxis,
                JNumChunk = mapSettings.NumChunk,
                JVerticesPos = verticesPos,
                JSortedVerticesPos = sortedVerticesPos,
            };
            JobHandle verticesSliceJobHandle = verticesSliceJob.ScheduleParallel(sq(mapSettings.NumChunk), JobsUtility.JobWorkerCount - 1, dependencySystem);
            verticesSliceJobHandle.Complete();
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct VerticesSliceJob : IJobFor
    {
        [ReadOnly] public int JMapPointPerAxis;
        [ReadOnly] public int JChunkPointPerAxis;
        [ReadOnly] public int JNumChunk;

        [ReadOnly]public NativeArray<float3> JVerticesPos;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float3> JSortedVerticesPos;
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
                    JSortedVerticesPos[sliceIndex] = JVerticesPos[start + x];
                }
            }
        }
    }
}
