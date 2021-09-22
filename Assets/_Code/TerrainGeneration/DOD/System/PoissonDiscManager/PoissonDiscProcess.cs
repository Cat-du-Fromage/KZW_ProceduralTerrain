using System.Collections;
using System.Collections.Generic;
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

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    public partial class MapSystem : MonoBehaviour
    {
        private float3[] pdcs;
        private void PoissonDiscProcess(in JobHandle dependency = new JobHandle())
        {
            using(NativeArray<float3> poissonDiscPos = AllocNtvAry<float3>(sq(mapSettings.NumCellMap)))
            {
                using (NativeArray<int> poissonDiscId = AllocNtvAry<int>(sq(mapSettings.NumCellMap)))
                {
                    PoissonDiscPerCellJob poissonDiscPerCellJob = new PoissonDiscPerCellJob
                    {
                        JSize = mapSettings.MapSize,
                        JCellSize = mapSettings.CellSize,
                        IndexInRow = mapSettings.NumCellMap,
                        Row = mapSettings.NumCellMap,
                        Seed = mapSettings.Seed,
                        JPoissonDiscPosition = poissonDiscPos,
                        JPoissonDiscId = poissonDiscId,
                    };
                    JobHandle jobHandle = poissonDiscPerCellJob.ScheduleParallel(sq(mapSettings.NumCellMap), JobsUtility.JobWorkerCount - 1, dependency);
                    jobHandle.Complete();
                    JsonHelper.ToJson<float3>(poissonDiscPos, dir.GetFullMapFileAt((int)FullMapFiles.PoissonDiscPos));
                    JsonHelper.ToJson<int>(poissonDiscId, dir.GetFullMapFileAt((int)FullMapFiles.PoissonDiscId));
                    pdcs = new float3[sq(mapSettings.NumCellMap)];
                    poissonDiscPos.CopyTo(pdcs);
                }
            }
        }


    }
    
    [BurstCompile(CompileSynchronously = true)]
    public struct PoissonDiscPerCellJob : IJobFor
    {
        [ReadOnly] public float JSize;
        [ReadOnly] public int JCellSize;
        //[ReadOnly] public float CellSize; //(w)radius/math.sqrt(2)
        [ReadOnly] public int IndexInRow; // X(cols) : math.floor(mapHeight/cellSize)
        [ReadOnly] public int Row; // Y(rows) :math.floor(mapWidth/cellSize)
        [ReadOnly] public uint Seed;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float3> JPoissonDiscPosition;
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<int> JPoissonDiscId;

        public void Execute(int index)
        {
            Random Prng = Random.CreateFromIndex(Seed + (uint)index);

            int cellPosY = (int)floor((float)index / Row);
            int cellPosX = index - (cellPosY * IndexInRow);
            float midSize = JSize / 2f;
            
            // Get the current Position of the center of the cell
            float midCellSize = JCellSize / 2f;
            float cellCenterX = mad(cellPosX, JCellSize, midCellSize);
            float cellCenterY = mad(cellPosY, JCellSize, midCellSize);
            float2 midCellPos = float2(cellCenterX, cellCenterY);
            
            //Process Random
            float2 randDirection = Prng.NextFloat2Direction();
            float2 sample = mad(randDirection, Prng.NextFloat(0 , midCellSize), midCellPos);
            
            JPoissonDiscPosition[index] = float3(sample.x, 0, sample.y)/* - float3(midSize,0,midSize)*/;
            JPoissonDiscId[index] = index;
        }
    }
}
