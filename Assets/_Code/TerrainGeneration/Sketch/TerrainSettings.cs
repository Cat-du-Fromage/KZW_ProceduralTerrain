using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.AddressableAssets;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.TerrainGeneration.VerticesSystem;
using static KaizerWaldCode.Utils.AddressablesUtils;

namespace KaizerWaldCode.TerrainGeneration
{
    public class TerrainSettings : MonoBehaviour
    {
        [Min(1)]
        [SerializeField] private int chunkSize;
        [Min(1)]
        [SerializeField] private int numChunk;
        [Range(2, 10)]
        [SerializeField] private int pointPerMeter;

        [SerializeField] private AssetReference computeShaderAsset;

        public int ChunkSize { get; private set; }
        public int NumChunk { get; private set; }
        public int PointPerMeter { get; private set; }

        public int MapSize { get; private set; }
        public int ChunkPointPerAxis { get; private set; }
        public int MapPointPerAxis { get; private set; }
        public float PointSpacing { get; private set; }
        //public Action<AsyncOperationHandle<ComputeShader>> OnLoadDone { get; private set; }

        public float3[] VerticesPosition;

        public int[] VerticesCellIndex;

#if UNITY_EDITOR
        private void OnValidate()
        {
            MapSize = chunkSize * numChunk;
            PointSpacing = 1f / (pointPerMeter - 1f);
            ChunkPointPerAxis = (chunkSize * pointPerMeter) - (chunkSize - 1);
            MapPointPerAxis = (numChunk * chunkSize) * pointPerMeter - (numChunk * chunkSize - 1);
        }
#endif

        private void Awake()
        {
            ChunkSize = max(1, chunkSize);
            NumChunk = max(1, numChunk);
            PointPerMeter = MinMax(2, 10);

            MapSize = chunkSize * numChunk;
            PointSpacing = 1f / (pointPerMeter - 1f);
            ChunkPointPerAxis = (chunkSize * pointPerMeter) - (chunkSize - 1);
            MapPointPerAxis = (numChunk * chunkSize) * pointPerMeter - (numChunk * chunkSize - 1);
        }

        private async void Start()
        {
            /*
            //Addressables.LoadAssetAsync<ComputeShader>("Assets/_Code/TerrainGeneration/ComputeShaders/VerticesProcess.compute").Completed += OnLoadDone;
            AsyncOperationHandle<ComputeShader> csHandle = Addressables.LoadAssetAsync<ComputeShader>(computeShaderAsset);
            csHandle.WaitForCompletion();
            if (csHandle.IsDone)
            {
                (VerticesPosition, VerticesCellIndex) = await TestArray(csHandle.Result);
                Addressables.Release(csHandle);
            }
            */
            AsyncOperationHandle<ComputeShader> csHandle = LoadSingleAssetSync<ComputeShader>(computeShaderAsset);
            (VerticesPosition, VerticesCellIndex) = await TestArray(csHandle.Result);
            Addressables.Release(csHandle);
        }


        //Test for array
        private async Task<(float3[], int[])> TestArray(ComputeShader cs)
        {
            int radius = 2;
            int numCell = (int)ceil(ChunkSize / (float)max(1, radius) * NumChunk);
            return await InitGridVertices(cs, MapPointPerAxis, MapSize, numCell, radius, PointSpacing);
        }
    }
}
