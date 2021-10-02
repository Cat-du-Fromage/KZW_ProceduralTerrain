using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using KaizerWaldCode.TerrainGeneration.Data;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using KaizerWaldCode.Utils;

using dir = KaizerWaldCode.TerrainGeneration.Directories_MapGeneration;

namespace KaizerWaldCode.TerrainGeneration
{
    public class BuildMeshesState : IState
    {
        readonly SettingsData mapSettings;
        private NativeArray<int> chunkTriangles;
        
        public ChunksData[] chunksData;

        public BuildMeshesState(in SettingsData mapSettings)
        {
            this.mapSettings = mapSettings;
        }
        
        public void DoState()
        {
            if (!(chunksData is null)) BuildMeshesProcess();
        }
        
        void BuildMeshesProcess()
        {
            chunkTriangles = AllocNtvAry<int>(sq(mapSettings.ChunkPointPerAxis-1) * 6);
            chunkTriangles.CopyFrom(JsonHelper.FromJson<int>(dir.GetFile_ChunksSharedTriangles()));
            
            for (int i = 0; i < chunksData.Length; i++)
            {
                NewMeshAPITest(i);
            }
            chunkTriangles.Dispose();
        }
        
        void NewMeshAPITest(int index)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.name = $"ChunkMesh{chunksData[index].Id}";
                
            using NativeArray<float3> verticesPos = new NativeArray<float3>(sq(mapSettings.ChunkPointPerAxis), Allocator.Temp);
            using NativeArray<float2> uvs = new NativeArray<float2>(sq(mapSettings.ChunkPointPerAxis), Allocator.Temp);
            
            verticesPos.CopyFrom(JsonHelper.FromJson<float3>(dir.GetFile_ChunkXYAt(chunksData[index].Position,ChunkFiles.VerticesPos)));
            uvs.CopyFrom(JsonHelper.FromJson<float2>(dir.GetFile_ChunkXYAt(chunksData[index].Position,ChunkFiles.Uvs)));
            
            VertexAttributeDescriptor[] layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                //new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 2),
                //new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.UNorm8, 4),
            };
            mesh.SetVertexBufferParams(verticesPos.Length, layout);
            mesh.SetVertexBufferData(verticesPos, 0, 0, verticesPos.Length, 0);
            mesh.SetIndexBufferParams(chunkTriangles.Length, IndexFormat.UInt32);
            mesh.SetIndexBufferData(chunkTriangles, 0, 0, chunkTriangles.Length);
            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor() {
                baseVertex = 0,
                bounds = default,
                indexStart = 0,
                indexCount = chunkTriangles.Length,
                firstVertex = 0,
                topology = MeshTopology.Triangles,
                vertexCount = verticesPos.Length
            });
            
            mesh.UploadMeshData(false);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
            
            chunksData[index].GetComponent<MeshFilter>().sharedMesh = mesh;
            chunksData[index].GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
}
