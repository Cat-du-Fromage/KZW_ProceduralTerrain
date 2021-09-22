using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.noise;
using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using Random = Unity.Mathematics.Random;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    /// <summary>
    /// Perlin Noise Process
    /// </summary>
    public partial class MapSystem : MonoBehaviour
    {
        private void PerlinNoiseProcess(in JobHandle dependency = new JobHandle())
        {
            using (noiseOffsetsMap = AllocNtvAry<float2>(noiseSettings.Octaves))
            {
                OffsetNoiseRandomJob offsetsNoiseJ = new OffsetNoiseRandomJob
                {
                    JSeed = mapSettings.Seed,
                    JOffset = noiseSettings.Offset,
                    OctOffsetArrayJob = noiseOffsetsMap,
                };
                JobHandle offsetsJH = offsetsNoiseJ.ScheduleParallel(noiseSettings.Octaves,JobsUtility.JobWorkerCount - 1, dependency);
                //============
                //PERLIN NOISE
                //============
                using (perlinNoiseMap = AllocNtvAry<float>(sq(mapSettings.MapPointPerAxis)))
                {
                    PerlinNoiseJob perlinNoiseJ = new PerlinNoiseJob
                    {
                        JNumPointPerAxis = mapSettings.MapPointPerAxis,
                        JOctaves = noiseSettings.Octaves,
                        JLacunarity = noiseSettings.Lacunarity,
                        JPersistance = noiseSettings.Persistence,
                        JScale = noiseSettings.Scale,
                        JHeightMul = noiseSettings.HeightMultiplier,
                        JOctOffsetArray = noiseOffsetsMap,
                        JNoiseMap = perlinNoiseMap,
                    };
                    JobHandle perlinNoiseJH = perlinNoiseJ.ScheduleParallel(sq(mapSettings.MapPointPerAxis), JobsUtility.JobWorkerCount - 1, offsetsJH);
                    perlinNoiseJH.Complete();
                    JsonHelper.ToJson<float>(perlinNoiseMap, dir.GetFullMapFileAt((int)FullMapFiles.Noise));
                    /*
                    using (sortedPerlinNoiseMap = AllocNtvAry<float>(sq(mapSettings.NumChunk) * sq(mapSettings.ChunkPointPerAxis)))
                    {
                        NoiseMapSliceJob noiseMapSliceJ = new NoiseMapSliceJob
                        {
                            JMapPointPerAxis = mapSettings.MapPointPerAxis,
                            JChunkPointPerAxis = mapSettings.ChunkPointPerAxis,
                            JNumChunk = mapSettings.NumChunk,
                            JUnsortedPerlinNoise = perlinNoiseMap,
                            JSortedPerlinNoise = sortedPerlinNoiseMap,
                        };
                        JobHandle noiseMapSliceJH = noiseMapSliceJ.ScheduleParallel(sq(mapSettings.NumChunk),JobsUtility.JobWorkerCount - 1, perlinNoiseJH);
                        noiseMapSliceJH.Complete();
                        JsonHelper.ToJson<float>(perlinNoiseMap, dir.GetFullMapFileAt((int)FullMapFiles.Noise));
                        for (int i = 0; i < chunks.Length; i++)
                        {
                            //Use chunk Id since we can't be certain of the order of chunks (oop..)
                            int start = chunks[i].Id * sq(mapSettings.ChunkPointPerAxis);
                            JsonHelper.ToJson(sortedPerlinNoiseMap.GetSubArray(start, sq(mapSettings.ChunkPointPerAxis)),dir.GetChunkFileAt(chunks[i].Position, (int)ChunkFiles.PerlinNoise));
                        }
                    }
                    */
                }
            }
        }
    }
    
    /// <summary>
    /// Process RandomJob
    /// </summary>
    [BurstCompile(CompileSynchronously = true)]
    public struct OffsetNoiseRandomJob : IJobFor
    {
        [ReadOnly] public uint JSeed;
        [ReadOnly] public float2 JOffset;
        [NativeDisableParallelForRestriction][WriteOnly] public NativeArray<float2> OctOffsetArrayJob;
        public void Execute(int index)
        {
            Random prng = Random.CreateFromIndex(JSeed);
            float offsetX = prng.NextFloat(-100000, 100000) + JOffset.x;
            float offsetY = prng.NextFloat(-100000, 100000) - JOffset.y;
            OctOffsetArrayJob[index] = float2(offsetX, offsetY);
        }
    }

    /// <summary>
    /// Noise Height Map
    /// </summary>
    [BurstCompile(CompileSynchronously = true)]
    public struct PerlinNoiseJob : IJobFor
    {
        [ReadOnly] public int JNumPointPerAxis;
        [ReadOnly] public int JOctaves;
        [ReadOnly] public float JLacunarity;
        [ReadOnly] public float JPersistance;
        [ReadOnly] public float JScale;
        [ReadOnly] public float JHeightMul;
        [ReadOnly] public NativeArray<float2> JOctOffsetArray;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float> JNoiseMap;

        public void Execute(int index)
        {
            float halfMapSize = mul(JNumPointPerAxis, 0.5f);

            int y = (int)floor((float)index / JNumPointPerAxis);
            int x = index - mul(y, JNumPointPerAxis); // or x = i % width;

            float amplitude = 1f;
            float frequency = 1f;
            float noiseHeight = 0;
            //Not needed in parallel! it's a layering of noise so it must be done contigiously
            for (int i = 0; i < JOctaves; i++)
            {
                float sampleX = mul((x - halfMapSize + JOctOffsetArray[i].x) / JScale, frequency);
                float sampleY = mul((y - halfMapSize + JOctOffsetArray[i].y) / JScale, frequency);
                float2 sampleXY = float2(sampleX, sampleY);

                float pNoiseValue = snoise(sampleXY);
                noiseHeight = mad(pNoiseValue, amplitude, noiseHeight);
                amplitude = mul(amplitude, JPersistance);
                frequency = mul(frequency, JLacunarity);
            }
            //NoiseMap[index] = math.mul(math.abs(math.lerp(0, 1f, noiseHeight)), HeightMulJob);
            float noiseVal = noiseHeight;
            noiseVal = abs(noiseVal);
            JNoiseMap[index] = mul(noiseVal, max(1,JHeightMul));
        }
    }
    /*
    [BurstCompile(CompileSynchronously = true)]
    public struct NoiseMapSliceJob : IJobFor
    {
        [ReadOnly] public int JMapPointPerAxis;
        [ReadOnly] public int JChunkPointPerAxis;
        [ReadOnly] public int JNumChunk;
        
        [ReadOnly] public NativeArray<float> JUnsortedPerlinNoise;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float> JSortedPerlinNoise;
        public void Execute(int index)
        {
            int chunkPosY = (int)floor(index / (float)JNumChunk);
            int chunkPosX = index - mul(chunkPosY, JNumChunk);

            for (int z = 0; z < JChunkPointPerAxis; z++) // z axis
            {
                int startY = (chunkPosY * JMapPointPerAxis) * (JChunkPointPerAxis - 1); //*chunksPoint-1 because of the height of the chunk; -1 because we need the vertice before
                int startX = chunkPosX * (JChunkPointPerAxis - 1);
                int startYChunk = z * JMapPointPerAxis; // y point relative to chunk (NOT CHUNK to MAP)
                int start = startY + startX + startYChunk;

                for (int x = 0; x < JChunkPointPerAxis; x++) // x axis
                {
                    int sliceIndex = mad(z, JChunkPointPerAxis, x) + (index * sq(JChunkPointPerAxis));
                    JSortedPerlinNoise[sliceIndex] = JUnsortedPerlinNoise[start + x];
                }
            }
        }
    }
    */
}
