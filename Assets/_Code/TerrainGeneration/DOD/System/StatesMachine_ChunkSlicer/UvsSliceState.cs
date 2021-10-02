using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.Utils;
using Unity.VisualScripting;
using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;

using dir = KaizerWaldCode.TerrainGeneration.Directories_MapGeneration;

namespace KaizerWaldCode.TerrainGeneration
{
    public class UvsSliceState : IState
    {
        readonly SettingsData mapSettings;
        
        public ChunksData[] chunksData;
        
        public UvsSliceState(in SettingsData mapSettings)
        {
            this.mapSettings = mapSettings;
        }
        
        public void DoState()
        {
            UvsSliceProcess();
        }

        void UvsSliceProcess()
        {
            //Uvs MAP process
            using NativeArray<float2> uvs = AllocNtvAry<float2>(sq(mapSettings.MapPointPerAxis)); // MAP POINTS
            using NativeArray<float2> sortedUvs = AllocNtvAry<float2>(sq(mapSettings.NumChunk) * sq(mapSettings.ChunkPointPerAxis));
            uvs.CopyFrom(JsonHelper.FromJson<float2>(dir.GetFile_MapAt(MapFiles.Uvs)));
            
            SortedUvsMeshJob job = new SortedUvsMeshJob
            {
                JMapPointPerAxis = mapSettings.MapPointPerAxis,
                JChunkPointPerAxis = mapSettings.ChunkPointPerAxis,
                JNumChunk = mapSettings.NumChunk,
                JUvs = uvs,
                JSortedUvs = sortedUvs
            };
            JobHandle jobHandle = job.ScheduleParallel(sq(mapSettings.NumChunk), JobsUtility.JobWorkerCount - 1, new JobHandle());
            jobHandle.Complete();
            
            SaveUvsInEachChunks(sortedUvs);
        }

        void SaveUvsInEachChunks(NativeArray<float2> sortedUvs)
        {
            for (int i = 0; i < chunksData.Length; i++)
            {
                //Use chunk Id since we can't be certain of the order of chunks (oop..)
                int start = chunksData[i].Id * sq(mapSettings.ChunkPointPerAxis);
                JsonHelper.ToJson(sortedUvs.GetSubArray(start, sq(mapSettings.ChunkPointPerAxis)), dir.GetFile_ChunkXYAt(chunksData[i].Position,ChunkFiles.Uvs));
            }
        }
        
        /// <summary>
        /// Sorted array for each chunk
        /// iteration : NumChunk^2 (total chunk)
        /// CAREFULL : Size of the SortedArray must be : (number Of Chunks axis)^2 * (Number of Points on Chunks axis)^2
        /// (or total chunks on map * total points on one chunk)
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        struct SortedUvsMeshJob : IJobFor
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
    }
}
