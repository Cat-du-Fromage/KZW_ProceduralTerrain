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
using Random = Unity.Mathematics.Random;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    /// <summary>
    /// Island Coast Process
    /// </summary>
    public partial class MapSystem : MonoBehaviour
    {
        void IslandCoastProcess(in JobHandle dependency = new JobHandle())
        {
            using (NativeArray<int> islandCoastID = AllocNtvAry<int>(sq(mapSettings.NumCellMap)))
            {
                NativeArray<float3> poissonDiscPosition = AllocNtvAry<float3>(sq(mapSettings.NumCellMap));
                poissonDiscPosition.CopyFrom( JsonHelper.FromJson<float3>(dir.GetFullMapFileAt((int)FullMapFiles.PoissonDiscPos)) );

                IslandCoastJob islandCoastJ = new IslandCoastJob
                {
                    JMapSize = mapSettings.MapSize,
                    JSeed = mapSettings.Seed,
                    JPoissonDiscPosition = poissonDiscPosition,
                    JIslandPoissonDiscID = islandCoastID
                };
                JobHandle islandCoastJH = islandCoastJ.ScheduleParallel(sq(mapSettings.NumCellMap), JobsUtility.JobWorkerCount - 1, dependency);
                islandCoastJH.Complete();
                JsonHelper.ToJson<int>(islandCoastID, dir.GetFullMapFileAt((int)FullMapFiles.Island));
                poissonDiscPosition.Dispose(islandCoastJH);
            }
        }
    }
    
    
    
    [BurstCompile(CompileSynchronously = true)]
    public struct IslandCoastJob : IJobFor
    {
        [ReadOnly] public int JMapSize;
        [ReadOnly] public uint JSeed;

        [ReadOnly] public NativeArray<float3> JPoissonDiscPosition;
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<int> JIslandPoissonDiscID;

        public void Execute(int index)
        {
            Random islandRandom = Random.CreateFromIndex(JSeed);
            
            float ISLAND_FACTOR = 1.27f; // 1.0 means no small islands; 2.0 leads to a lot
            float PI2 = PI*2;
            float midSize = JMapSize / 2f; // need to be added because calculated on the center of the map(mapSize)
            float3 sampleDisc = JPoissonDiscPosition[index]/* + float3(midSize, 0, midSize)*/;
            
            float x = 2f * (sampleDisc.x / JMapSize - 0.5f);
            float z = 2f * (sampleDisc.z / JMapSize - 0.5f);
            float3 point = new float3(x, 0, z);

            int bumps = islandRandom.NextInt(1, 6);
            float startAngle = islandRandom.NextFloat(PI2); //radians 2 Pi = 360°
            float dipAngle = islandRandom.NextFloat(PI2);
            float dipWidth = islandRandom.NextFloat(0.2f, 0.7f); // = mapSize?

            float angle = atan2(point.z, point.x);
            float lengthMul = 0.5f; // 0.1f : big island 1.0f = small island // by increasing by 0.1 island size is reduced by 1
            float totalLength = mad(lengthMul, max(abs(point.x), abs(point.z)), length(point));
            
            float radialsBase = mad(bumps, angle, startAngle); // bump(1-6) * angle(0.x) + startangle(0.x)
            float r1Sin = sin(radialsBase + cos((bumps + 3) * angle));
            float r2Sin = sin(radialsBase + sin((bumps + 2) * angle));
            
            float radial1 = 0.5f + 0.4f * r1Sin;
            float radial2 = 0.7f - 0.2f * r2Sin;

            if (abs(angle - dipAngle) < dipWidth || abs(angle - dipAngle + PI2) < dipWidth || abs(angle - dipAngle - PI2) < dipWidth)
            {
                radial1 = radial2 = 0.2f;
            }

            JIslandPoissonDiscID[index] = select(0,1,totalLength < radial1 || (totalLength > radial1 * ISLAND_FACTOR && totalLength < radial2));
        }
    }
}