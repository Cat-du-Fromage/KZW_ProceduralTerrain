using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.KWSerialization;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.TerrainGeneration.KwSystem;
using KaizerWaldCode.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using dir = KaizerWaldCode.Directories_MapGeneration;
using static Unity.Mathematics.math;
using static Unity.Mathematics.float3;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.KWSerialization.BinarySerialization;
using Debug = UnityEngine.Debug;

namespace KaizerWaldCode
{
    public class VerticesState : IState
    {
        readonly SettingsData mapSettings;
        readonly int mapTotalPoints;
        public VerticesState(SettingsData mapSettings)
        {
            this.mapSettings = mapSettings;
            mapTotalPoints = sq(mapSettings.MapPointPerAxis);
        }
        
        public void DoState()
        {
            VerticesPositionProcess();
            VerticesCellIndexProcess();
            SharedVerticesPositionProcess();
        }
        
        /// <summary>
        /// Process Shared Vertices for Chunks
        /// Size of the Array is : Chunk Size * Chunk Size
        /// </summary>
        /// <param name="dependency"></param>
        private void SharedVerticesPositionProcess(in JobHandle dependency = new JobHandle())
        {
            using NativeArray<float3> verticesPos = AllocNtvAry<float3>(sq(mapSettings.ChunkPointPerAxis));
            
            VerticesPosJob job = new VerticesPosJob
            {
                JSize = mapSettings.ChunkSize,
                JPointPerAxis = mapSettings.ChunkPointPerAxis,
                JSpacing = mapSettings.PointSpacing,
                JVertices = verticesPos,
            };
            JobHandle jobHandle = job.ScheduleParallel(sq(mapSettings.ChunkPointPerAxis), JobsUtility.JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            JsonHelper.ToJson<float3>(verticesPos, dir.GetFile_ChunksSharedVertex());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependency">dependency for the job(use an empty one if not needed)</param>
        private void VerticesPositionProcess(in JobHandle dependency = new JobHandle())
        {
            using NativeArray<float3> verticesPos = AllocNtvAry<float3>(mapTotalPoints);
            
            VerticesPosJob job = new VerticesPosJob
            {
                JSize = mapSettings.MapSize,
                JPointPerAxis = mapSettings.MapPointPerAxis,
                JSpacing = mapSettings.PointSpacing,
                JVertices = verticesPos,
            };
            JobHandle jobHandle = job.ScheduleParallel(mapTotalPoints, JobsUtility.JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            JsonHelper.ToJson<float3>(verticesPos, dir.GetFile_MapAt(MapFiles.VerticesPos));
        }

        private void VerticesCellIndexProcess(in JobHandle dependency = new JobHandle())
        {
            using NativeArray<int> verticesCellIndex = AllocFillNtvAry<int>(mapTotalPoints, -1);
            using NativeArray<float3> verticesPos = AllocNtvAry<float3>(mapTotalPoints);
            verticesPos.CopyFrom(JsonHelper.FromJson<float3>(dir.GetFile_MapAt(MapFiles.VerticesPos)));
            
            VerticesCellIndexJob job = new VerticesCellIndexJob
            {
                JNumCellMap = mapSettings.NumCellMap,
                JCellSize = mapSettings.CellSize,
                JVertices = verticesPos,
                JVerticesCellGrid = verticesCellIndex,
            };
            JobHandle jobHandle = job.ScheduleParallel(mapTotalPoints, JobsUtility.JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            JsonHelper.ToJson<int>(verticesCellIndex, dir.GetFile_MapAt(MapFiles.VerticesCellIndex));
        }
        
        
    }
    
    #region JOBS

        /// <summary>
        /// Process Vertices Positions
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        public struct VerticesPosJob : IJobFor
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

                //float3 pointPosition = mad(float3(x, 0, z), float3(JSpacing), float3(-midSize,0,-midSize));
                float3 pointPosition = float3(x, 0, z) * float3(JSpacing);
                JVertices[index] = pointPosition;
            }
        }

        /// <summary>
        /// Process Vertices Cell Index
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        public struct VerticesCellIndexJob : IJobFor
        {
            [ReadOnly] public int JNumCellMap;
            [ReadOnly] public int JCellSize;
            [ReadOnly] public NativeArray<float3> JVertices;

            [NativeDisableParallelForRestriction]
            [WriteOnly] public NativeArray<int> JVerticesCellGrid;
            public void Execute(int index)
            {
                float2 cellGrid = float2(JNumCellMap);
                float2 currVertPos = JVertices[index].xz;

                FindCell(ref cellGrid, in currVertPos);
                JVerticesCellGrid[index] = (int)mad(cellGrid.y, JNumCellMap, cellGrid.x);
            }

            void FindCell(ref float2 cellGrid, in float2 vertPos)
            {
                for (int i = 0; i < JNumCellMap; i++)
                {
                    if ((int)cellGrid.y == JNumCellMap) cellGrid.y = select(JNumCellMap, i, vertPos.y <= mad(i, JCellSize, JCellSize));
                    if ((int)cellGrid.x == JNumCellMap) cellGrid.x = select(JNumCellMap, i, vertPos.x <= mad(i, JCellSize, JCellSize));
                    if ((int)cellGrid.x != JNumCellMap && (int)cellGrid.y != JNumCellMap) break;
                }
            }
        }

        #endregion JOBS
}
