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
using static KaizerWaldCode.Utils.JsonHelper;

using dir = KaizerWaldCode.TerrainGeneration.Directories_MapGeneration;

namespace KaizerWaldCode.TerrainGeneration
{
    public class SharedTrianglesState : IState
    {
        readonly SettingsData mapSettings;
        public SharedTrianglesState(in SettingsData mapSettings)
        {
            this.mapSettings = mapSettings;
        }
        
        public void DoState()
        {
            TrianglesProcess();
        }
        
        private void TrianglesProcess()
        {
            /*
                triangles process
                DONT FORGET * 6!! 
                mapSettings.ChunkPointPerAxis-1 represent the umber of "quads" on the grid (CARFUL : number quads != numchunks or chunkSize (because of the numPointPerMeter)
            */
            using NativeArray<int> triangles = AllocNtvAry<int>(sq(mapSettings.ChunkPointPerAxis-1) * 6);
            TrianglesMeshJob job = new TrianglesMeshJob
            {
                JChunkPointPerAxis = mapSettings.ChunkPointPerAxis,
                JTriangles = triangles
            };
            //CAREFULL we need to gro through all points(DO NOT make width - 1 and height - 1!), we filter them in the job!
            JobHandle jobHandle = job.ScheduleParallel(sq(mapSettings.ChunkPointPerAxis), JobsUtility.JobWorkerCount - 1, new JobHandle());
            jobHandle.Complete();
            ToJson(triangles, dir.GetFile_ChunksSharedTriangles());
        }

        [BurstCompile(CompileSynchronously = true)]
        struct TrianglesMeshJob : IJobFor
        {
            [ReadOnly] public int JChunkPointPerAxis;

            [NativeDisableParallelForRestriction] [WriteOnly] public NativeArray<int> JTriangles;
            public void Execute(int index)
            {
                int y = (int)floor((float)index / JChunkPointPerAxis);
                int x = index - mul(y, JChunkPointPerAxis);
                int baseTriIndex = index * 6;
            
                if (y < (JChunkPointPerAxis-1) && x < (JChunkPointPerAxis-1) )
                {
                    int triangleIndex = select(baseTriIndex, baseTriIndex - (6 * y), y != 0);
                    //[i1] [i2] [i3] [i4] [i5] [i6] | [01] [02] [03] [04] [05] [06]  | [j1] [j2] [j3] [j4] [j5] [j6]
                    //y=0                           | y=0                            |  y=1
                    //tri 1                         | gap(x < (JChunkPointPerAxis-1))| replace j1 to 01 : currentTriangleIndex - (6*y) => baseTriIndex - (6 * y)
                    int4 trianglesVertex = int4(index, index + JChunkPointPerAxis + 1, index + JChunkPointPerAxis, index + 1);
                    JTriangles[triangleIndex] = trianglesVertex.z;
                    JTriangles[triangleIndex + 1] = trianglesVertex.y;
                    JTriangles[triangleIndex + 2] = trianglesVertex.x;
                    triangleIndex += 3;
                    JTriangles[triangleIndex] = trianglesVertex.w;
                    JTriangles[triangleIndex + 1] = trianglesVertex.x;
                    JTriangles[triangleIndex + 2] = trianglesVertex.y;
                }
            }
        }
    }
}
