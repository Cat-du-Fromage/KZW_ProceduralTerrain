using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using KaizerWaldCode.TerrainGeneration.Data;
using Unity.VisualScripting;
using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using KaizerWaldCode.Utils;

using dir = KaizerWaldCode.TerrainGeneration.Directories_MapGeneration;

namespace KaizerWaldCode.TerrainGeneration
{
    public class UvsState : IState
    {
        readonly SettingsData mapSettings;
        readonly int totalMapPoints;
        
        public UvsState(in SettingsData mapSettings)
        {
            this.mapSettings = mapSettings;
            totalMapPoints = sq(mapSettings.MapPointPerAxis);
        }
        
        public void DoState()
        {
            ProcessUvs();
        }

        void ProcessUvs()
        {
            using NativeArray<float2> uvs = AllocNtvAry<float2>(totalMapPoints); // MAP POINTS
            UvsMeshJob job = new UvsMeshJob
            {
                JMapPointPerAxis = mapSettings.MapPointPerAxis,
                JUvs = uvs
            };
            JobHandle jobHandle = job.ScheduleParallel(totalMapPoints, JobsUtility.JobWorkerCount - 1, new JobHandle());
            jobHandle.Complete();
            JsonHelper.ToJson(uvs, dir.GetFile_MapAt(MapFiles.Uvs));
        }
        
        [BurstCompile(CompileSynchronously = true)]
        struct UvsMeshJob : IJobFor
        {
            [ReadOnly] public int JMapPointPerAxis;
        
            [NativeDisableParallelForRestriction] [WriteOnly] public NativeArray<float2> JUvs;
            public void Execute(int index)
            {
                float y = floor((float)index / JMapPointPerAxis);
                float x = index - (y * JMapPointPerAxis);
                JUvs[index] = float2(x / JMapPointPerAxis, y / JMapPointPerAxis);
            }
        }
    }
}
