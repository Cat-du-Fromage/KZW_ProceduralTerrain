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
using static Unity.Mathematics.float3;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    public partial class MapSystem : MonoBehaviour
    {
        private JobHandle VerticesProcess(JobHandle dependencySystem , NativeArray<float3> vertPos, NativeArray<int> vertIndex, SettingsData data)
        {
            VerticesProcessJob verticesProcessJob = new VerticesProcessJob
            {
                JMapPointPerAxis = data.MapPointPerAxis,
                JSpacing = data.PointSpacing,
                JNumCellMap = data.MapSize/2,
                JRadius = 2,
                JVertices = vertPos,
                JVerticesCellGrid = vertIndex
            };
            JobHandle verticesProcessJobHandle = verticesProcessJob.ScheduleParallel(vertPos.Length, JobsUtility.JobWorkerCount - 1, dependencySystem);
            return verticesProcessJobHandle;
        }
    }

    [BurstCompile]
    public struct VerticesProcessJob : IJobFor
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
}
