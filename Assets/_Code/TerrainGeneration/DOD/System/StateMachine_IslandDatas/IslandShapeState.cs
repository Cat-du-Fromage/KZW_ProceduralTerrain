using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using KaizerWaldCode.TerrainGeneration.Data;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using Random = Unity.Mathematics.Random;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;

using dir = KaizerWaldCode.TerrainGeneration.Directories_MapGeneration;

namespace KaizerWaldCode.TerrainGeneration
{
    public class IslandShapeState : IState
    {
        readonly SettingsData mapSettings;
        
        public IslandShapeState(in SettingsData mapSettings)
        {
            this.mapSettings = mapSettings;
        }
        
        public void DoState()
        {
            IslandShapeProcess();
        }
        
        void IslandShapeProcess(in JobHandle dependency = new JobHandle())
        {
            using NativeArray<int> islandCoastID = AllocNtvAry<int>(sq(mapSettings.NumCellMap));
            using NativeArray<float3> poissonDiscPosition = AllocNtvAry<float3>(sq(mapSettings.NumCellMap));
            poissonDiscPosition.CopyFrom( JsonHelper.FromJson<float3>(dir.GetFile_MapAt(MapFiles.PoissonDiscPos)) );

            IslandCoastJob job = new IslandCoastJob
            {
                JMapSize = mapSettings.MapSize,
                JSeed = mapSettings.Seed,
                JPoissonDiscPosition = poissonDiscPosition,
                JIslandPoissonDiscID = islandCoastID
            };
            JobHandle jobHandle = job.ScheduleParallel(sq(mapSettings.NumCellMap), JobsUtility.JobWorkerCount - 1, dependency);
            jobHandle.Complete();

            using NativeList<int> tempContainer = new NativeList<int>(Allocator.Temp);
            for (int i = 0; i < islandCoastID.Length; i++)
            {
                if (islandCoastID[i] == -1) continue;
                tempContainer.Add(islandCoastID[i]);
            }
            JsonHelper.ToJson<int>(tempContainer.ToArray(), dir.GetFile_MapAt(MapFiles.Island));
        }
        
        [BurstCompile(CompileSynchronously = true)]
        struct IslandCoastJob : IJobFor
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

                float angle = atan2(point.z, point.x); //more like : where am i on the circle
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

                JIslandPoissonDiscID[index] = select(-1,index,totalLength < radial1 || (totalLength > radial1 * ISLAND_FACTOR && totalLength < radial2));
            }
        }
    }
}
