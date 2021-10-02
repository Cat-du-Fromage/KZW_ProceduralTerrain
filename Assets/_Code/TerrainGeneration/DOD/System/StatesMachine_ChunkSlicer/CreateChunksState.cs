using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.Utils;
using Unity.Mathematics;
using UnityEngine.ResourceManagement.AsyncOperations;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.AddressablesUtils;
using static KaizerWaldCode.Utils.KWmath;

namespace KaizerWaldCode.TerrainGeneration
{
    public class CreateChunksState : IState
    {
        const string chunkAsset = "TerrainChunk";
        GameObject[] chunksIngame;
        ChunksData[] chunksDatas;
        
        readonly SettingsData mapSettings;
        readonly int totalNumChunk;

        readonly float meshBoundSize;
        readonly float mapCenterOffset;

        public CreateChunksState(in SettingsData mapSettings)
        {
            this.mapSettings = mapSettings;
            chunksIngame = GameObject.FindGameObjectsWithTag("Map Chunk");
            totalNumChunk = sq(mapSettings.NumChunk);
            meshBoundSize = chunksIngame.Length != 0 ? chunksIngame[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.x : 0;
            mapCenterOffset = (mapSettings.ChunkSize / 2f) * (mapSettings.NumChunk - 1);
        }
        
        public void DoState()
        {
            CreateChunkProcess();
        }
        
        void CreateChunkProcess()
        {
            //Use to center the map on (0,0,0)
            //Vector3 offset = new Vector3(mapCenterOffset, 0, mapCenterOffset);
            if (chunksIngame.Length != totalNumChunk)
            {
                if (chunksIngame.Length != 0) { DestroyAllChunks(ref chunksIngame); }
                chunksDatas = new ChunksData[totalNumChunk];
                AsyncOperationHandle<GameObject> chunkAssetRef = Addressables.LoadAssetAsync<GameObject>(chunkAsset);
                chunkAssetRef.WaitForCompletion();
                
                for (int i = 0; i < totalNumChunk; i++)
                {
                    (int posX, int posY) = KwGrid.GetXY(in i, in mapSettings.NumChunk);
                    GameObject chunkGameObj = InitChunkGameObject(chunkAssetRef.Result, posX, posY);
                    ChunksData chunkObjData = InitChunksData(in i, in posX, in posY, ref chunkGameObj);
                    chunksDatas[i] = chunkObjData;
                }
                Addressables.Release(chunkAssetRef);
            }
            //RePosition Chunk when size is changed but not the number
            else if ( ((int)meshBoundSize != mapSettings.ChunkSize) && (meshBoundSize != 0) )
            {
                for (int i = 0; i < chunksIngame.Length; i++)
                {
                    (int posX, int posY) = KwGrid.GetXY(in i, in mapSettings.NumChunk);
                    chunksIngame[i].transform.position = new Vector3(posX * mapSettings.ChunkSize, 0, posY * mapSettings.ChunkSize);
                }
            }
        }

        GameObject InitChunkGameObject(GameObject prefab, in int x, in int y)
        {
            GameObject obj = GameObject.Instantiate(prefab);
            obj.name = $"Chunk ({x}; {y})";
            obj.transform.position = new Vector3(x * mapSettings.ChunkSize, 0, y * mapSettings.ChunkSize);
            return obj;
        }
        
        /// <summary>
        /// Initialise chunksData of a given Chunk GameObject
        /// </summary>
        /// <param name="index">index in the loop</param>
        /// <param name="x">X grid position</param>
        /// <param name="y">Y grid position</param>
        /// <param name="chunk">chunk Gameobject</param>
        /// <returns>ChunksData</returns>
        ChunksData InitChunksData(in int index, in int x, in int y ,ref GameObject chunkObj)
        {
            ChunksData chunkObjData = chunkObj.GetComponent<ChunksData>();
            chunkObjData.Id = index;
            chunkObjData.Position = int2(x, y);

            return chunkObjData;
        }
        
        /// <summary>
        /// Destroy all chunk GameObjects
        /// </summary>
        /// <param name="chunksGameObject">Array of Chunks GameObject</param>
        void DestroyAllChunks(ref GameObject[] chunksGameObject)
        {
            for (int i = 0; i < chunksGameObject.Length; i++)
            {
                GameObject.Destroy(chunksGameObject[i]);
            }
        }
    }
}
