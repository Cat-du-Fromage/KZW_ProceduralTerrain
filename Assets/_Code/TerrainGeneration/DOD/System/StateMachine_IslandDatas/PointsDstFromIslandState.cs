using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.TerrainGeneration;
using KaizerWaldCode.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using KaizerWaldCode.TerrainGeneration.Data;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.KwGrid;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using dir = KaizerWaldCode.TerrainGeneration.Directories_MapGeneration;

namespace KaizerWaldCode.TerrainGeneration
{
    public class PointsDstFromIslandState : IState, IDisposable
    {
        //private NativeArray<int> islandPointsId;
        readonly SettingsData mapSettings;
        readonly int totalMapPoints;
        readonly int totalChunkPoints;
        //Read
        NativeArray<int> islandPointsId; //length can't be known so we can't use NativeArray
        NativeArray<int> voronoies;
        //Write
        NativeArray<int> dstFromIsland;

        NativeList<int> previousDstSets;

        public PointsDstFromIslandState(in SettingsData mapSettings)
        {
            this.mapSettings = mapSettings;
            totalMapPoints = sq(in mapSettings.MapPointPerAxis);
            totalChunkPoints = sq(in mapSettings.ChunkPointPerAxis);
        }
        
        public void DoState()
        {
            SetIslandPoints();
            Dispose();
        }

        void PointsDstFromIslandProcess()
        {
            //Declare NativeList<int>
            
            //Use the List to Find
            
            //Dispose NativeList
        }

        void SetIslandPoints()
        {
            //Get IslandPoints Index
            int[] iPIds = JsonHelper.FromJson<int>(dir.GetFile_MapAt(MapFiles.Island));
            islandPointsId = AllocNtvAry<int>(iPIds.Length);
            islandPointsId.CopyFrom(iPIds);
            //Load Voronoies Array
            voronoies = AllocNtvAry<int>(totalMapPoints);
            voronoies.CopyFrom(JsonHelper.FromJson<int>(dir.GetFile_MapAt(MapFiles.Voronoi)));
            //Get Indices Related to thoses IslandPoints
            dstFromIsland = AllocNtvAry<int>(totalMapPoints);
            previousDstSets = new NativeList<int>(iPIds.Length * totalChunkPoints, Allocator.TempJob);
            JobHandle dependency = GetIslandPoints();
            dependency.Complete();
            JsonHelper.ToJson(dstFromIsland, dir.GetFile_MapAt(MapFiles.PointsDistance));
            //Make an array Size total Vertices AND Set all of them to -1;
            //Use Voronoi for Check

            //Set Points at indices of IslandsPoints To(check with voronoies) : 0
            //Store Index of points Set to 0 into a List!
        }

        void SetDstPoints()
        {
            //Get Indices List of IslandPoints
            //
            JobHandle dependency = new JobHandle();
            List<JobHandle> jobHandles = new List<JobHandle>();
            jobHandles.Add(dependency);
            int currentDst = 1;
            for (int i = 0; i < 1; i++)
            {
                DstFromIslandJob job = new DstFromIslandJob();
                job.JDst = currentDst;
                jobHandles.Add(job.ScheduleParallel(10, JobsUtility.JobWorkerCount - 1, jobHandles[currentDst-1]));
                currentDst++;
            }
            /*
            while (test.Contains(-1))
            {
                DstFromIslandJob job = new DstFromIslandJob();
                job.Jdst = currentDst;
                jobHandles.Add(job.ScheduleParallel(10, JobsUtility.JobWorkerCount - 1, jobHandles[currentDst-1]));
                currentDst++;
            }
            */
        }
        
        
        JobHandle GetIslandPoints(JobHandle dependency = new JobHandle())
        {
            SetIslandPointsJob job = new SetIslandPointsJob
            {
                JIslandIds = islandPointsId,
                Jvoronoies = voronoies,
                JDstFromIsland = dstFromIsland,
                JCurrentsDst = previousDstSets.AsParallelWriter()
            };
            JobHandle jobHandle = job.ScheduleParallel(totalMapPoints, JobsUtility.JobWorkerCount - 1, dependency);
            return jobHandle;
        }
        
        [BurstCompile(CompileSynchronously = true)]
        struct SetIslandPointsJob : IJobFor
        {
            [ReadOnly] public NativeArray<int> JIslandIds;
            [ReadOnly] public NativeArray<int> Jvoronoies;
            [NativeDisableParallelForRestriction]
            [WriteOnly] public NativeArray<int> JDstFromIsland;
            [NativeDisableParallelForRestriction]
            [WriteOnly] public NativeList<int>.ParallelWriter JCurrentsDst;
            public void Execute(int index)
            {
                bool containIsland = JIslandIds.Contains(Jvoronoies[index]);
                
                JDstFromIsland[index] = select(-1,0,containIsland);
                if (containIsland) JCurrentsDst.AddNoResize(Jvoronoies[index]);
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        struct DstFromIslandJob : IJobFor
        {
            [ReadOnly] public float JDst;
            [ReadOnly] public int JMapChunkPoints;
            
            public NativeArray<int> JIslandIds;
            public NativeArray<int> Jvoronoies;
            public NativeArray<int> JDstFromIsland;
            public void Execute(int index)
            {
                (int x, int y) = GetXY(in index,in JMapChunkPoints);
                
                //CellGridRanges(cellId, JNumCell)
            }
        }

        public void Dispose()
        {
            if (dstFromIsland.IsCreated) dstFromIsland.Dispose();
            if (voronoies.IsCreated) voronoies.Dispose();
            if (previousDstSets.IsCreated) previousDstSets.Dispose();
            if (islandPointsId.IsCreated) islandPointsId.Dispose();
        }
    }
}
