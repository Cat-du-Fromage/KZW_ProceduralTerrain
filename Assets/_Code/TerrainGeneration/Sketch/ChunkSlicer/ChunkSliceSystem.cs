using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KaizerWaldCode.Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Unity.Mathematics.math;

namespace KaizerWaldCode.TerrainGeneration
{
    public class ChunkSliceSystem : MonoBehaviour
    {
        [SerializeField]private TerrainSettings ms;

        private List<GameObject> ob = new List<GameObject>();
        [SerializeField]private AssetReference chunk;
        private AsyncOperationHandle<GameObject> hold;

        private AsyncOperationHandle<GameObject> CreateChunks()
        {
            AsyncOperationHandle<GameObject> t = Addressables.LoadAssetAsync<GameObject>(chunk);
            return t;
        }

        /*
        private void Test()
        {
            float3[] Vertices;
            int chunkPoints = 10; //ChunkPointPerAxis
            int numChunks = 10;
            int mapPoints = 10;

            float3[] chunkHolderBuffer = new float3[100]; // all vertices previously calculated

            //Chunk Position RELATIVE TO MAP!!!
            for (int entityInQueryIndex = 0; entityInQueryIndex < 100; entityInQueryIndex++)
            {
                //GET Position x,z instead
                int posY = (int)floor((float)entityInQueryIndex / (float)numChunks);
                int posX = entityInQueryIndex - posY * numChunks;

                //POINT Position RELATIVE TO CHUNK!!!
                NativeArray<float3> verticesSlice = new NativeArray<float3>(chunkPoints * chunkPoints, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                for (int z = 0; z < chunkPoints; z++) // z axis
                {
                    int startY =(posY * mapPoints) * (chunkPoints - 1); //*chunksPoint-1 because of the height of the chunk; -1 because we need the vertice before

                    int startX = posX * (chunkPoints - 1);

                    int startYChunk = z * mapPoints; // y point relative to chunk (NOT CHUNK to MAP)

                    int start = startY + startX + startYChunk;

                    for (int x = 0; x < chunkPoints; x++) // x axis
                    {
                        verticesSlice[z * chunkPoints + x] = chunkHolderBuffer[start + x];
                    }
                }
                //vertices.AddRange(verticesSlice);
                verticesSlice.Dispose();
            }
        }
        */

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log($"terrain Set {ms.ChunkSize}; chunkNum {ms.NumChunk}");
            int nChunk = 3;
            int nChunks = KWmath.sq(nChunk);

            hold = CreateChunks();
            hold.WaitForCompletion();
            for (int i = 0; i < nChunks; i++)
            {
                int posY = (int)floor((float)i / (float)nChunk);
                int posX = i - posY * nChunk;

                GameObject go = Instantiate(hold.Result);
                go.transform.localPosition = new Vector3(posX*10,0, posY*10);
                ob.Add(go);
            }
        }

        void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame && hold.IsValid())
            {
                Debug.Log("l pressed");
                Addressables.ReleaseInstance(hold);
            }
        }

    }
}
