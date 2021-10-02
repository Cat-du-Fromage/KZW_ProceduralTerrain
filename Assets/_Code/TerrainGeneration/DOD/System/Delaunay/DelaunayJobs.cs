using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.Utils.KWmath;

using float2 = Unity.Mathematics.float2;

namespace KaizerWaldCode.TerrainGeneration.KwDelaunay
{
    [BurstCompile(CompileSynchronously = true)]
    public struct DelaunayProcessIdsJob : IJobFor
    {
        [NativeDisableParallelForRestriction][WriteOnly] public NativeArray<int> JIds;
        public void Execute(int index)
        {
            JIds[index] = index;
        }
    }
    
    [BurstCompile(CompileSynchronously = true)]
    public struct DstPointsToCircumCenterJob : IJobFor
    {
        [ReadOnly] public float2 JCircumcenterPos;
        [ReadOnly] public NativeArray<float3> JPoissonPos;
        [NativeDisableParallelForRestriction][WriteOnly] public NativeArray<float> JDst;
        public void Execute(int index)
        {
            JDst[index] = distancesq(JCircumcenterPos, JPoissonPos[index].xz);
        }
    }
}
