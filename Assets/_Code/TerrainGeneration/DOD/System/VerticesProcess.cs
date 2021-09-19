using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using KaizerWaldCode.KWSerialization;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static Unity.Mathematics.float3;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.KWSerialization.BinarySerialization;
using Debug = UnityEngine.Debug;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    public partial class MapSystem : MonoBehaviour
    {
        /// <summary>
        /// Process Shared Vertices for Chunks
        /// Size of the Array is : Chunk Size * Chunk Size
        /// </summary>
        /// <param name="dependency"></param>
        private void SharedVerticesPositionProcess(in JobHandle dependency = new JobHandle())
        {
            using(verticesPos = AllocNtvAry<float3>(sq(mapSettings.ChunkPointPerAxis)))
            {
                VerticesPosProcessJob vertPosJob = new VerticesPosProcessJob
                {
                    JSize = mapSettings.ChunkSize,
                    JPointPerAxis = mapSettings.ChunkPointPerAxis,
                    JSpacing = mapSettings.PointSpacing,
                    JVertices = verticesPos,
                };
                JobHandle vertPosJobHandle = vertPosJob.ScheduleParallel(sq(mapSettings.ChunkPointPerAxis), JobsUtility.JobWorkerCount - 1, dependency);
                vertPosJobHandle.Complete();
                JsonHelper.ToJson<float3>(verticesPos, dir.GetChunksSharedVertexFile());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependency">dependency for the job(use an empty one if not needed)</param>
        private void VerticesPositionProcess(in JobHandle dependency = new JobHandle())
        {
            using(verticesPos = AllocNtvAry<float3>(mapPointSurface))
            {
                VerticesPosProcessJob vertPosJob = new VerticesPosProcessJob
                {
                    JSize = mapSettings.MapSize,
                    JPointPerAxis = mapSettings.MapPointPerAxis,
                    JSpacing = mapSettings.PointSpacing,
                    JVertices = verticesPos,
                };
                JobHandle vertPosJobHandle = vertPosJob.ScheduleParallel(mapPointSurface, JobsUtility.JobWorkerCount - 1, dependency);
                vertPosJobHandle.Complete();
                JsonHelper.ToJson<float3>(verticesPos, dir.GetFullMapFileAt((int)FullMapFiles.VerticesPos));
            }
        }

        private void VerticesCellIndexProcess(in JobHandle dependency = new JobHandle())
        {
            verticesCellIndex = AllocFillNtvAry<int>(mapPointSurface, -1);
            verticesPos = AllocNtvAry<float3>(mapPointSurface);

            verticesPos.CopyFrom(JsonHelper.FromJson<float3>(dir.GetFullMapFileAt((int) FullMapFiles.VerticesPos)));
            VerticesCellIndexProcessJob vertCellIdJob = new VerticesCellIndexProcessJob
            {
                JNumCellMap = mapSettings.NumCellMap,
                JRadius = mapSettings.Radius,
                JVertices = verticesPos,
                JVerticesCellGrid = verticesCellIndex,
            };
            JobHandle vertCellIdJobHandle = vertCellIdJob.ScheduleParallel(mapPointSurface, JobsUtility.JobWorkerCount - 1, dependency);
            vertCellIdJobHandle.Complete();
            JsonHelper.ToJson<int>(verticesCellIndex, dir.GetFullMapFileAt((int)FullMapFiles.VerticesCellIndex));

            verticesPos.Dispose();
            verticesCellIndex.Dispose();
        }
    }

    #region JOBS

    /// <summary>
    /// Process Vertices Positions
    /// </summary>
    [BurstCompile(CompileSynchronously = true)]
    public struct VerticesPosProcessJob : IJobFor
    {
        //MapSettings
        [ReadOnly] public int JSize;
        [ReadOnly] public int JPointPerAxis;
        [ReadOnly] public float JSpacing;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float3> JVertices;
        public void Execute(int index)
        {
            int z = (int)floor(index / (float)JPointPerAxis);
            int x = index - (z * JPointPerAxis);
            float midSize = JSize / 2f;

            float3 pointPosition = mad(float3(x, 0, z), float3(JSpacing), float3(-midSize,0,-midSize));
            JVertices[index] = pointPosition;
        }
    }

    /// <summary>
    /// Process Vertices Cell Index
    /// </summary>
    [BurstCompile(CompileSynchronously = true)]
    public struct VerticesCellIndexProcessJob : IJobFor
    {
        [ReadOnly] public int JNumCellMap;
        [ReadOnly] public int JRadius;
        [ReadOnly] public NativeArray<float3> JVertices;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<int> JVerticesCellGrid;
        public void Execute(int index)
        {
            float2 cellGrid = float2(JNumCellMap);
            float2 currVertPos = JVertices[index].xz;

            FindCell(ref cellGrid, currVertPos);
            JVerticesCellGrid[index] = (int)mad(cellGrid.y, JNumCellMap, cellGrid.x);
        }

        void FindCell(ref float2 cellGrid, float2 vertPos)
        {
            for (int i = 0; i < JNumCellMap; i++)
            {
                if ((int)cellGrid.y == JNumCellMap) cellGrid.y = select(JNumCellMap, i, vertPos.y <= mad(i, JRadius, JRadius));
                if ((int)cellGrid.x == JNumCellMap) cellGrid.x = select(JNumCellMap, i, vertPos.x <= mad(i, JRadius, JRadius));
                if ((int)cellGrid.x != JNumCellMap && (int)cellGrid.y != JNumCellMap) break;
            }
        }
    }

    #endregion JOBS
}
