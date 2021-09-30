using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.TerrainGeneration.KwSystem;
using KaizerWaldCode.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;
using Random = Unity.Mathematics.Random;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;

using dir = KaizerWaldCode.Directories_MapGeneration;

namespace KaizerWaldCode
{
    public class RandomPointsState : IState
    {
        readonly SettingsData mapSettings;
        readonly int totalCellMap;

        public RandomPointsState(in SettingsData mapSettings)
        {
            this.mapSettings = mapSettings;
            totalCellMap = sq(mapSettings.NumCellMap);
        }
        
        public void DoState()
        {
            PoissonDiscProcess();
        }
        
        private void PoissonDiscProcess(in JobHandle dependency = new JobHandle())
        {
            using NativeArray<float3> poissonDiscPos = AllocNtvAry<float3>(totalCellMap);
            using NativeArray<int> poissonDiscId = AllocNtvAry<int>(totalCellMap);
            
            RandomPointsJob job = new RandomPointsJob
            {
                JSize = mapSettings.MapSize,
                JCellSize = mapSettings.CellSize,
                JIndexInRow = mapSettings.NumCellMap,
                JRow = mapSettings.NumCellMap,
                JSeed = mapSettings.Seed,
                JRandomPointsPosition = poissonDiscPos,
                JRandomPointsId = poissonDiscId,
            };
            JobHandle jobHandle = job.ScheduleParallel(totalCellMap, JobsUtility.JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            
            JsonHelper.ToJson<float3>(poissonDiscPos, dir.GetFile_MapAt(MapFiles.PoissonDiscPos));
            JsonHelper.ToJson<int>(poissonDiscId, dir.GetFile_MapAt(MapFiles.PoissonDiscId));
        }
    }
    
    [BurstCompile(CompileSynchronously = true)]
    public struct RandomPointsJob : IJobFor
    {
        [ReadOnly] public float JSize;
        [ReadOnly] public int JCellSize;
        //[ReadOnly] public float CellSize; //(w)radius/math.sqrt(2)
        [ReadOnly] public int JIndexInRow; // X(cols) : math.floor(mapHeight/cellSize)
        [ReadOnly] public int JRow; // Y(rows) :math.floor(mapWidth/cellSize)
        [ReadOnly] public uint JSeed;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float3> JRandomPointsPosition;
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<int> JRandomPointsId;

        public void Execute(int index)
        {
            Random Prng = Random.CreateFromIndex(JSeed + (uint)index);

            int cellPosY = (int)floor((float)index / JRow);
            int cellPosX = index - (cellPosY * JIndexInRow);
            float midSize = JSize / 2f;
            
            // Get the current Position of the center of the cell
            float midCellSize = JCellSize / 2f;
            float cellCenterX = mad(cellPosX, JCellSize, midCellSize);
            float cellCenterY = mad(cellPosY, JCellSize, midCellSize);
            float2 midCellPos = float2(cellCenterX, cellCenterY);
            
            //Process Random
            float2 randDirection = Prng.NextFloat2Direction();
            float2 sample = mad(randDirection, Prng.NextFloat(0 , midCellSize), midCellPos);
            
            JRandomPointsPosition[index] = float3(sample.x, 0, sample.y)/* - float3(midSize,0,midSize)*/;
            JRandomPointsId[index] = index;
        }
    }
}
