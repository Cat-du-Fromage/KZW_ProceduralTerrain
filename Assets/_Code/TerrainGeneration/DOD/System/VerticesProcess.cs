using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.KWSerialization;
using KaizerWaldCode.TerrainGeneration.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static Unity.Mathematics.float3;
using static KaizerWaldCode.Utils.KWmath;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    public partial class MapSystem : MonoBehaviour
    {
        private JobHandle VerticesDoubleProcess(in JobHandle dependencySystem)
        {
            VerticesPosAndIndexProcessJob verticesPosAndIndexProcessJob = new VerticesPosAndIndexProcessJob
            {
                JMapPointPerAxis = mapSettings.MapPointPerAxis,
                JSpacing = mapSettings.PointSpacing,
                JNumCellMap = mapSettings.NumCellMap,
                JRadius = mapSettings.Radius,
                JVertices = verticesPos,
                JVerticesCellGrid = verticesCellIndex
            };
            JobHandle verticesPosAndIndexProcessJobHandle = verticesPosAndIndexProcessJob.ScheduleParallel(mapPointSurface, JobsUtility.JobWorkerCount - 1, dependencySystem);
            return verticesPosAndIndexProcessJobHandle;
        }

        private void VerticesPositionProcess(in JobHandle dependencySystem)
        {
            VerticesPosProcessJob verticesPosProcessJob = new VerticesPosProcessJob
            {
                JMapPointPerAxis = mapSettings.MapPointPerAxis,
                JSpacing = mapSettings.PointSpacing,
                JVertices = verticesPos,
            };
            JobHandle verticesPosProcessJobHandle = verticesPosProcessJob.ScheduleParallel(mapPointSurface, JobsUtility.JobWorkerCount - 1, dependencySystem);
            verticesPosProcessJobHandle.Complete();
            //return verticesPosProcessJobHandle;
        }

        private void VerticesCellIndexProcess(in JobHandle dependencySystem)
        {
            VerticesCellIndexProcessJob verticesCellIndexProcessJob = new VerticesCellIndexProcessJob
            {
                JNumCellMap = mapSettings.MapPointPerAxis,
                JRadius = mapSettings.Radius,
                JVertices = verticesPos,
                JVerticesCellGrid = verticesCellIndex,
            };
            JobHandle verticesCellIndexProcessJobHandle = verticesCellIndexProcessJob.ScheduleParallel(mapPointSurface, JobsUtility.JobWorkerCount - 1, dependencySystem);
            verticesCellIndexProcessJobHandle.Complete();
            //return verticesCellIndexProcessJobHandle;
        }
    }

    #region JOBS

    /// <summary>
    /// Process Both Vertices Positions and Cell Index
    /// </summary>
    [BurstCompile(CompileSynchronously = true)]
    public struct VerticesPosAndIndexProcessJob : IJobFor
    {
        //MapSettings
        [ReadOnly] public int JMapPointPerAxis;
        [ReadOnly] public float JSpacing;
        //PoissonDisc
        [ReadOnly] public int JNumCellMap;
        [ReadOnly] public int JRadius;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float3> JVertices;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<int> JVerticesCellGrid;
        public void Execute(int index)
        {
            int z = (int)floor(index / (float)JMapPointPerAxis);
            int x = index - (z * JMapPointPerAxis);

            float3 pointPosition = float3(x, 0, z) * float3(JSpacing);
            JVertices[index] = pointPosition;

            float2 cellGrid = float2(JNumCellMap);
            float2 currVertPos = pointPosition.xz;

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

    /// <summary>
    /// Process Vertices Positions
    /// </summary>
    [BurstCompile(CompileSynchronously = true)]
    public struct VerticesPosProcessJob : IJobFor
    {
        //MapSettings
        [ReadOnly] public int JMapPointPerAxis;
        [ReadOnly] public float JSpacing;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float3> JVertices;
        public void Execute(int index)
        {
            int z = (int)floor(index / (float)JMapPointPerAxis);
            int x = index - (z * JMapPointPerAxis);


            float3 pointPosition = float3(x, 0, z) * float3(JSpacing);
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
