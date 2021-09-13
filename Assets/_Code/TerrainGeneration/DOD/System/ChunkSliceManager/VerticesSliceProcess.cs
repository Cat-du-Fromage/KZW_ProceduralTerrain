using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //Vertices Position
            verticesPos = AllocNtvAry<float3>(sq(mapSettings.MapPointPerAxis));
            verticesPos.CopyFrom(Load<float3>(dir.GetFullMapFileAt((int)FullMapFiles.VerticesPos)));
            sortedVerticesPos = AllocNtvAry<float3>(sq(mapSettings.NumChunk) * sq(mapSettings.ChunkPointPerAxis));

            //Vertices Cell Index
            verticesCellIndex = AllocNtvAry<int>(sq(mapSettings.MapPointPerAxis));
            verticesCellIndex.CopyFrom(Load<int>(dir.GetFullMapFileAt((int)FullMapFiles.VerticesCellIndex)));
            sortedVerticesCellIndex = AllocNtvAry<int>(sq(mapSettings.NumChunk) * sq(mapSettings.ChunkPointPerAxis));
            //VerticesPosSliceJobProcess(gDependency);
            VerticesPosSliceJobProcess(gDependency);

            for (int i = 0; i < chunks.Length; i++)
            {
                ChunksData chunkData = chunks[i].GetComponent<ChunksData>();
                //Use chunk Id since we can't be certain of the order of chunks (oop..)
                int start = chunkData.Id * sq(mapSettings.ChunkPointPerAxis);

                Save<float3>(dir.GetChunkFileAt(chunkData.Position, (int)ChunkFiles.VerticesPos),
                               sortedVerticesPos.GetSubArray(start, sq(mapSettings.ChunkPointPerAxis)).ToArray());
                Save<int>(dir.GetChunkFileAt(chunkData.Position, (int)ChunkFiles.VerticesCellIndex),
                            sortedVerticesCellIndex.GetSubArray(start, sq(mapSettings.ChunkPointPerAxis)).ToArray());
            }

            verticesPos.Dispose();
            sortedVerticesPos.Dispose();
            sw.Stop();
            UnityEngine.Debug.Log($"VerticesSliceProcess {sw.Elapsed}");
            verticesCellIndex.Dispose();
            sortedVerticesCellIndex.Dispose();
        }

        private void VerticesCellIndexSliceProcess()
        {
            verticesCellIndex = AllocNtvAry<int>(sq(mapSettings.MapPointPerAxis));
            verticesCellIndex.CopyFrom(Load<int>(dir.GetFullMapFileAt((int)FullMapFiles.VerticesCellIndex)));

            sortedVerticesCellIndex = AllocNtvAry<int>(sq(mapSettings.NumChunk) * sq(mapSettings.ChunkPointPerAxis));
            VerticesPosSliceJobProcess(gDependency);
            for (int i = 0; i < chunks.Length; i++)
            {
                ChunksData chunkData = chunks[i].GetComponent<ChunksData>();
                //Use chunk Id since we can't be certain of the order of chunks (oop..)
                int start = chunkData.Id * sq(mapSettings.ChunkPointPerAxis);
                Save<int>(dir.GetChunkFileAt(chunkData.Position, (int)ChunkFiles.VerticesCellIndex),
                            sortedVerticesCellIndex.GetSubArray(start, sq(mapSettings.ChunkPointPerAxis)).ToArray());
            }

            verticesCellIndex.Dispose();
            sortedVerticesCellIndex.Dispose();
        }


        private void VerticesPosSliceJobProcess(in JobHandle dependencySystem)
        {
            VerticesSliceJob verticesSliceJob = new VerticesSliceJob
            {
                JMapPointPerAxis = mapSettings.MapPointPerAxis,
                JChunkPointPerAxis = mapSettings.ChunkPointPerAxis,
                JNumChunk = mapSettings.NumChunk,
                JVerticesPos = verticesPos,
                JSortedVerticesPos = sortedVerticesPos,
                JVerticesId = verticesCellIndex,
                JSortedVerticesId = sortedVerticesCellIndex,
            };
            JobHandle verticesSliceJobHandle = verticesSliceJob.ScheduleParallel(sq(mapSettings.NumChunk), JobsUtility.JobWorkerCount - 1, dependencySystem);
            verticesSliceJobHandle.Complete();
        }

        private void VerticesIdSliceJobProcess(in JobHandle dependencySystem)
        {
            VerticesIndexSliceJob verticesIndexSliceJob = new VerticesIndexSliceJob
            {
                JMapPointPerAxis = mapSettings.MapPointPerAxis,
                JChunkPointPerAxis = mapSettings.ChunkPointPerAxis,
                JNumChunk = mapSettings.NumChunk,
                JVerticesId = verticesCellIndex,
                JSortedVerticesId = sortedVerticesCellIndex,
            };
            JobHandle verticesIndexSliceJobHandle = verticesIndexSliceJob.ScheduleParallel(sq(mapSettings.NumChunk), JobsUtility.JobWorkerCount - 1, dependencySystem);
            verticesIndexSliceJobHandle.Complete();
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct VerticesSliceJob : IJobFor
    {
        [ReadOnly] public int JMapPointPerAxis;
        [ReadOnly] public int JChunkPointPerAxis;
        [ReadOnly] public int JNumChunk;

        [ReadOnly]public NativeArray<float3> JVerticesPos;
        [ReadOnly] public NativeArray<int> JVerticesId;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float3> JSortedVerticesPos;
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
                    JSortedVerticesPos[sliceIndex] = JVerticesPos[start + x];
                    JSortedVerticesId[sliceIndex] = JVerticesId[start + x];
                }
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct VerticesIndexSliceJob : IJobFor
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
