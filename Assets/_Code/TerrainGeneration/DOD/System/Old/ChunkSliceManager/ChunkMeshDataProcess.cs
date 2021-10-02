using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.Utils.JsonHelper;
using Debug = UnityEngine.Debug;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    /// <summary>
    /// Chunk Mesh Data Process
    /// </summary>
    public partial class MapSystem : MonoBehaviour
    {
        private void MeshDatasProcess()
        {
            /*
                triangles process
                DONT FORGET * 6!! 
                mapSettings.ChunkPointPerAxis-1 represent the umber of "quads" on the grid (CARFUL : number quads != numchunks or chunkSize (because of the numPointPerMeter)
            */
            triangles = AllocNtvAryOption<int>(sq(mapSettings.ChunkPointPerAxis-1) * 6, NativeArrayOptions.ClearMemory); 
            JobHandle triJobHandle = TrianglesMeshProcess(gDependency, ref triangles);
            
            //Uvs MAP process
            uvs = AllocNtvAry<float2>(sq(mapSettings.MapPointPerAxis)); // MAP POINTS
            JobHandle uvsJobHandle = UvsMeshProcess(triJobHandle, ref uvs);
            uvsJobHandle.Complete();
            
            ToJson(triangles, dir.GetChunksTriangleFile());
            ToJson(uvs, dir.GetFullMapFileAt((int)MapFiles.Uvs));
            triangles.Dispose(); // free some memory
            
            sortedUvs = AllocNtvAry<float2>(sq(mapSettings.NumChunk) * sq(mapSettings.ChunkPointPerAxis));
            JobHandle sortedUvsJobHandle = SortedUvsMeshProcess(uvsJobHandle, uvs, ref sortedUvs);
            sortedUvsJobHandle.Complete();
            
            //Distribute Uvs to all chunks
            for (int i = 0; i < chunks.Length; i++)
            {
                //Triangles DONT NEED ATTRIBUTION! (see : MeshCreationProcess.cs)
                //Use chunk Id since we can't be certain of the order of chunks (oop..)
                int start = chunks[i].Id * sq(mapSettings.ChunkPointPerAxis);
                ToJson(sortedUvs.GetSubArray(start, sq(mapSettings.ChunkPointPerAxis)), dir.GetChunkFileAt(chunks[i].Position, (int)ChunkFiles.Uvs));
            }
            uvs.Dispose();
            sortedUvs.Dispose();
        }
        
        /// <summary>
        /// TRIANGLES
        /// </summary>
        /// <param name="dependency"></param>
        /// <param name="tri"></param>
        /// <returns></returns>
        private JobHandle TrianglesMeshProcess(in JobHandle dependency, ref NativeArray<int> tri)
        {
            TrianglesMeshJob triJob = new TrianglesMeshJob
            {
                JChunkPointPerAxis = mapSettings.ChunkPointPerAxis,
                JTriangles = tri
            };//CAREFULL we need to gro through all points, we filter them in the job!
            JobHandle jobHandle = triJob.ScheduleParallel(sq(mapSettings.ChunkPointPerAxis), JobsUtility.JobWorkerCount - 1, dependency);
            return jobHandle;
        }
        
        private JobHandle UvsMeshProcess(in JobHandle dependency, ref NativeArray<float2> unsortedUvs)
        {
            UvsMeshJob uvsJob = new UvsMeshJob
            {
                JMapPointPerAxis = mapSettings.MapPointPerAxis,
                JUvs = uvs
            };
            JobHandle jobHandle = uvsJob.ScheduleParallel(sq(mapSettings.MapPointPerAxis), JobsUtility.JobWorkerCount - 1, dependency);
            return jobHandle;
        }
        
        private JobHandle SortedUvsMeshProcess(in JobHandle dependency, in NativeArray<float2> unsortedUvs, ref NativeArray<float2> newSortedUvs)
        {
            SortedUvsMeshJob sortedUvsJob = new SortedUvsMeshJob
            {
                JMapPointPerAxis = mapSettings.MapPointPerAxis,
                JChunkPointPerAxis = mapSettings.ChunkPointPerAxis,
                JNumChunk = mapSettings.NumChunk,
                JUvs = unsortedUvs,
                JSortedUvs = newSortedUvs
            };
            JobHandle jobHandle = sortedUvsJob.ScheduleParallel(sq(mapSettings.NumChunk), JobsUtility.JobWorkerCount - 1, dependency);
            return jobHandle;
        }
    }
    /// <summary>
    /// TRIANGLES
    /// </summary>
    [BurstCompile(CompileSynchronously = true)]
    public struct TrianglesMeshJob : IJobFor
    {
        [ReadOnly] public int JChunkPointPerAxis;

        [NativeDisableParallelForRestriction] [WriteOnly] public NativeArray<int> JTriangles;
        public void Execute(int index)
        {
            int y = (int)floor((float)index / (float)JChunkPointPerAxis);
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
    
    #region UVS DATA
    [BurstCompile(CompileSynchronously = true)]
    public struct UvsMeshJob : IJobFor
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
    /// <summary>
    /// Sorted array for each chunk
    /// iteration : NumChunk^2 (total chunk)
    /// CAREFULL : Size of the SortedArray must be : (number Of Chunks axis)^2 * (Number of Points on Chunks axis)^2
    /// (or total chunks on map * total points on one chunk)
    /// </summary>
    [BurstCompile(CompileSynchronously = true)]
    public struct SortedUvsMeshJob : IJobFor
    {
        [ReadOnly] public int JMapPointPerAxis;
        [ReadOnly] public int JChunkPointPerAxis;
        [ReadOnly] public int JNumChunk;

        [ReadOnly] public NativeArray<float2> JUvs;
        [NativeDisableParallelForRestriction] [WriteOnly] public NativeArray<float2> JSortedUvs;
        public void Execute(int index)
        {
            //CAREFUL: take index depending on chunk!
            int posY = (int)floor(index / (float)JNumChunk);
            int posX = index - (posY * JNumChunk);
            for (int y = 0; y < JChunkPointPerAxis; y++) // z axis
            {
                /*
                 * 0  0  0  0  0  0
                 * Y1 0  0  0  0  0 +(z * JMapPointPerAxis)
                 * Y  X  0  0  0  0 (posY * JMapPointPerAxis) * (JChunkPointPerAxis - 1) / posX * (JChunkPointPerAxis - 1);
                 * 1  1  0  0  0  0
                 * 1  1  0  0  0  0 
                 */
                int startY = (posY * JMapPointPerAxis) * (JChunkPointPerAxis - 1); //*chunksPoint-1 because of the height of the chunk; -1 because we need the vertice before
                int startX = posX * (JChunkPointPerAxis - 1);
                int startYChunk = y * JMapPointPerAxis; // y point relative to chunk (NOT CHUNK to MAP)
                int start = startY + startX + startYChunk;
                for (int x = 0; x < JChunkPointPerAxis; x++)
                {
                    int sliceIndex = mad(y, JChunkPointPerAxis, x) + (index * sq(JChunkPointPerAxis));
                    JSortedUvs[sliceIndex] = JUvs[start + x];
                }
            }
        }
    }
    #endregion UVS DATA
}
