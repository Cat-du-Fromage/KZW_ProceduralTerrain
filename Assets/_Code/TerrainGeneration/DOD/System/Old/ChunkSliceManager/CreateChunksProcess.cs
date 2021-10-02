using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KaizerWaldCode.TerrainGeneration.Data;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.AddressablesUtils;
using static KaizerWaldCode.Utils.KWmath;
using Debug = UnityEngine.Debug;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    public partial class MapSystem : MonoBehaviour
    {
        [SerializeField] private AssetReferenceGameObject chunk;

        void CreateChunkProcess()
        {
            GameObject[] chunksInGame = GameObject.FindGameObjectsWithTag("Map Chunk");
            float meshBoundSize = 0;
            float mapCenterOffset = (mapSettings.ChunkSize/2f) * (mapSettings.NumChunk-1);
            //Use to center the map on (0,0,0)
            Vector3 offset = new Vector3(mapCenterOffset, 0, mapCenterOffset);
            
            // Get Bound size of one chunk in game
            if (chunksInGame.Length != 0)
            {
                meshBoundSize = chunksInGame[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.x;
            }
                
            
            if (chunksInGame.Length != sq(mapSettings.NumChunk))
            {
                Debug.Log($"chunksInGame.Length = {chunksInGame.Length} || sq(mapSettings.NumChunk) = {sq(mapSettings.NumChunk)}");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                if (chunksInGame.Length != 0)
                {
                    DestroyAllChunks(chunksInGame);
                }
                
                int nbChunk = mapSettings.NumChunk;
                int sizeChunk = mapSettings.ChunkSize;
                chunks = new ChunksData[sq(mapSettings.NumChunk)];
                Debug.Log($"ChunkSize = {mapSettings.ChunkSize}");

                AsyncOperationHandle<GameObject> chunkAsset = LoadSingleAssetSync<GameObject>(chunk);

                for (int i = 0; i < sq(nbChunk); i++)
                {
                    int posY = (int)floor((float)i / nbChunk);
                    int posX = i - posY * nbChunk;

                    GameObject chunkGameObj = Instantiate(chunkAsset.Result);
                    chunkGameObj.name = $"Chunk ({posX}; {posY})";
                    chunkGameObj.transform.position = new Vector3(posX * sizeChunk, 0, posY * sizeChunk);

                    ChunksData chunkObjData = chunkGameObj.GetComponent<ChunksData>();
                    chunkObjData.Id = i;
                    chunkObjData.Position = int2(posX, posY);

                    chunks[i] = chunkObjData;
                }

                Addressables.Release(chunkAsset);
                sw.Stop();
                UnityEngine.Debug.Log($"CreateChunkProcess = {sw.Elapsed}");
            }
            else if ((int)meshBoundSize != mapSettings.ChunkSize && meshBoundSize != 0) //Replace chunks if only the size get changed(and not the number of chunks)
            {
                for (int i = 0; i < chunksInGame.Length; i++)
                {
                    int posY = (int)floor((float)i / mapSettings.NumChunk);
                    int posX = i - posY * mapSettings.NumChunk;
                    chunksInGame[i].transform.position = new Vector3(posX * mapSettings.ChunkSize, 0, posY * mapSettings.ChunkSize);
                }
            }
            
        }

        void DestroyAllChunks(GameObject[] chunksGo)
        {
            for (int i = 0; i < chunksGo.Length; i++)
            {
                Destroy(chunksGo[i]);
            }
        }

    }
}
