using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KaizerWaldCode.TerrainGeneration.Data;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.AddressablesUtils;
using static KaizerWaldCode.Utils.KWmath;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    public partial class MapSystem : MonoBehaviour
    {
        [SerializeField] private AssetReference chunk;

        void CreateChunkProcess()
        {
            int chunkInScene = GameObject.FindGameObjectsWithTag("Map Chunk").Length;
            if (chunkInScene != sq(mapSettings.NumChunk))
            {
                int nbChunk = mapSettings.NumChunk;
                int sizeChunk = mapSettings.ChunkSize;
                chunks = new GameObject[sq(nbChunk)];

                AsyncOperationHandle<GameObject> chunkAsset = LoadSingleAssetSync<GameObject>(chunk);

                for (int i = 0; i < sq(nbChunk); i++)
                {
                    int posY = (int)floor((float)i / (float)nbChunk);
                    int posX = i - posY * nbChunk;

                    GameObject chunkGameObj = Instantiate(chunkAsset.Result);
                    chunkGameObj.name = $"Chunk ({posX}; {posY})";
                    chunkGameObj.transform.localPosition = new Vector3(posX * sizeChunk, 0, posY * sizeChunk);

                    ChunksData chunkObjData = chunkGameObj.GetComponent<ChunksData>();
                    chunkObjData.Id = i;
                    chunkObjData.Position = int2(posX, posY);
                    chunkObjData.Vertices = new float3[sq(mapSettings.ChunkPointPerAxis)];

                    chunks[i] = chunkGameObj;
                }

                Addressables.Release(chunkAsset);
            }
        }

    }
}
