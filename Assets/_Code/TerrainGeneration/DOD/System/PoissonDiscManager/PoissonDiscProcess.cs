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
        private void PoissonDiscProcess(in JobHandle gDependency)
        {
            using (NativeArray<float3> poissonDiscPos = AllocNtvAry<float3>(sq(mapSettings.NumCellMap)))
            {
                PoissonDiscPerCellJob poissonDiscPerCellJob = new PoissonDiscPerCellJob
                {
                    JSize = mapSettings.MapSize,
                    RadiusJob = mapSettings.Radius,
                    IndexInRow = mapSettings.NumCellMap,
                    Row = mapSettings.NumCellMap,
                    Seed = mapSettings.Seed,
                    PoissonDiscPosition = poissonDiscPos
                };
                JobHandle jobHandle = poissonDiscPerCellJob.ScheduleParallel(sq(mapSettings.NumCellMap), JobsUtility.JobWorkerCount - 1, gDependency);
                jobHandle.Complete();
                JsonHelper.ToJson<float3>(poissonDiscPos, dir.GetFullMapFileAt((int)FullMapFiles.PoissonDiscPos));
                pdcs = new float3[sq(mapSettings.NumCellMap)];
                poissonDiscPos.CopyTo(pdcs);
            }
        }


    }

    
    [BurstCompile(CompileSynchronously = true)]
    public struct PoissonDiscGenerationJob : IJob
    {
        [ReadOnly] public int MapSize;

        [ReadOnly] public int NumSampleBeforeRejectJob;
        [ReadOnly] public float RadiusJob;
        [ReadOnly] public float CellSize; //(w)radius/math.sqrt(2)
        [ReadOnly] public int IndexInRow; // X(cols) : math.floor(mapHeight/cellSize)
        [ReadOnly] public int Row; // Y(rows) :math.floor(mapWidth/cellSize)
        [ReadOnly] public Random Prng;

        public NativeArray<int> DiscGridJob;
        public NativeList<float2> ActivePointsJob;
        public NativeList<float2> SamplePointsJob;

        public void Execute()
        {
            //float2 firstPoint = new float2(MapSize / 2f, MapSize / 2f);
            float2 firstPoint = float2.zero;
            ActivePointsJob.Add(firstPoint);
            while (!ActivePointsJob.IsEmpty)
            {
                int spawnIndex = Prng.NextInt(ActivePointsJob.Length);
                float2 spawnPosition = ActivePointsJob[spawnIndex];
                bool accepted = false;
                for (int k = 0; k < NumSampleBeforeRejectJob; k++)
                {
                    //Prng = Random.CreateFromIndex(JSeed);
                    float2 randDirection = Prng.NextFloat2Direction();
                    float2 sample = mad(randDirection, Prng.NextFloat(RadiusJob, mul(2, RadiusJob)), spawnPosition);

                    int sampleX = (int)(sample.x / CellSize); //col
                    int sampleY = (int)(sample.y / CellSize); //row
                    //TEST for rejection
                    if (SampleAccepted(sample, sampleX, sampleY))
                    {
                        SamplePointsJob.Add(sample);
                        ActivePointsJob.Add(sample);
                        DiscGridJob[mad(sampleY, Row, sampleX)] = SamplePointsJob.Length;
                        accepted = true;
                        break;
                    }
                }

                if (!accepted) ActivePointsJob.RemoveAt(spawnIndex);
            }

        }

        bool SampleAccepted(in float2 sample,in int sampleX, in int sampleY)
        {
            if (sample.x >= 0 && sample.x < MapSize && sample.y >= 0 && sample.y < MapSize)
            {
                int searchStartX = max(0, sampleX - 2);
                int searchEndX = min(sampleX + 2, IndexInRow - 1);

                int searchStartY = max(0, sampleY - 2);
                int searchEndY = min(sampleY + 2, Row - 1);

                // <= or it will created strange cluster of points at the borders of the map
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    for (int x = searchStartX; x <= searchEndX; x++)
                    {
                        int indexSample = DiscGridJob[mad(y, Row, x)] - 1;
                        if (indexSample != -1)
                        {
                            if (distancesq(sample, SamplePointsJob[indexSample]) < mul(RadiusJob, RadiusJob)) return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
    
    [BurstCompile(CompileSynchronously = true)]
    public struct PoissonDiscPerCellJob : IJobFor
    {
        [ReadOnly] public float JSize;
        [ReadOnly] public float RadiusJob;
        //[ReadOnly] public float CellSize; //(w)radius/math.sqrt(2)
        [ReadOnly] public int IndexInRow; // X(cols) : math.floor(mapHeight/cellSize)
        [ReadOnly] public int Row; // Y(rows) :math.floor(mapWidth/cellSize)
        [ReadOnly] public uint Seed;

        [WriteOnly] public NativeArray<float3> PoissonDiscPosition;

        public void Execute(int index)
        {
            Random Prng = Random.CreateFromIndex(Seed + (uint)index);

            int cellPosY = (int)floor(index / (float)Row);
            int cellPosX = index - (cellPosY * IndexInRow);
            float midSize = JSize / 2f;
            
            // Get the current Position of the center of the cell
            float midCellSize = RadiusJob / 2f;
            float cellCenterX = mad(cellPosX, RadiusJob, midCellSize);
            float cellCenterY = mad(cellPosY, RadiusJob, midCellSize);
            float2 midCellPos = float2(cellCenterX, cellCenterY);
            
            //Process Random
            float2 randDirection = Prng.NextFloat2Direction();
            float2 sample = mad(randDirection, Prng.NextFloat(0 , midCellSize), midCellPos);
            
            PoissonDiscPosition[index] = float3(sample.x, 0, sample.y) - float3(midSize,0,midSize);
        }
    }
}
