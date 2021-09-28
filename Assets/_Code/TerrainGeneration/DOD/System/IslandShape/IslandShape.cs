using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.TerrainGeneration.KwSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;

namespace KaizerWaldCode.TerrainGeneration
{
    public class IslandShape
    {
        MapDirectories dir;
        
        int numVertices;
        int[] islandPoissonDiscID;
        int[] voronoies;
        
        int[] dstVerticesFromIsland;

        public IslandShape(MapDirectories dir)
        {
            this.dir = dir;
        }


        //=====================================================
        //JOBS
        //=====================================================
        
        /// <summary>
        /// Get all vertices composing the island
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        public struct GetIslandPointsJob : IJobFor
        {
            [ReadOnly] public NativeArray<int> JIslandPoissDiscID;
            [ReadOnly] public NativeArray<int> JVoronoies;

            [NativeDisableParallelForRestriction]
            [WriteOnly] public NativeArray<int> JDstVerticesFromIsland;
            public void Execute(int index)
            {
                for (int i = 0; i < JIslandPoissDiscID.Length; i++)
                {
                    if (JVoronoies[index] == JIslandPoissDiscID[i])
                    {
                        JDstVerticesFromIsland[index] = 0;
                    }
                }
            }
        }
    }
}
