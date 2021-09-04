using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static KaizerWaldCode.Utils.ShaderUtils;
using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using static UnityEngine.Shader;

namespace KaizerWaldCode.TerrainGeneration
{
    public static class VerticesSystem
    {
        public async static Task<(float3[], int[])> InitGridVertices(ComputeShader initGridCShader, int mapPointPerAxis, int mapSize, int numCellMap, int radius, float pointSpacing)
        {
            float3[] verticesPosition = new float3[sq(mapPointPerAxis)];
            int[] cellIndexVertices = new int[sq(mapPointPerAxis)];

            (verticesPosition, cellIndexVertices) = await ComputeShaderInit(verticesPosition, cellIndexVertices, initGridCShader, mapPointPerAxis, mapSize, numCellMap, radius, pointSpacing);
            return (verticesPosition, cellIndexVertices);
        }

        static async Task<(float3[], int[])> ComputeShaderInit(float3[] vertArr, int[] cellIndexVert, ComputeShader cs, int mapPointPerAxis, int mapSize, int numCellMap, int radius, float pointSpacing)
        {
            cs.SetInt(PropertyToID("pointPerAxis"), mapPointPerAxis);
            cs.SetInt("mapSize", mapSize);
            cs.SetInt("numCellMap", numCellMap);
            cs.SetInt("radius", radius);
            cs.SetFloat("spacing", pointSpacing);
            
            using ComputeBuffer verticesBuffer = CreateAndSetBuffer<float3>(vertArr, cs, "grid");
            using ComputeBuffer cellIndexVertBuffer = CreateAndSetBuffer<int>(cellIndexVert, cs, "cellIndex");

            // out and ref parameters are not allowed in async method
            (vertArr, cellIndexVert) = await AsyncGpuRequest<float3, int>(cs, new int3(mapPointPerAxis, 1, mapPointPerAxis), verticesBuffer, cellIndexVertBuffer);
            return (vertArr, cellIndexVert);
        }
    }
}
