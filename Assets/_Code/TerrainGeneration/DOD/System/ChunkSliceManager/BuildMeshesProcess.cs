using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.Utils.JsonHelper;
using Debug = UnityEngine.Debug;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    /// <summary>
    /// Build Meshes Process
    /// </summary>
    public partial class MapSystem : MonoBehaviour
    {
        void BuildMeshesProcess()
        {
            chunkTriangles = AllocNtvAry<int>(sq(mapSettings.ChunkPointPerAxis-1) * 6, NativeArrayOptions.ClearMemory);
            chunkTriangles.CopyFrom(FromJson<int>(dir.GetChunksTriangleFile()));
            
            for (int i = 0; i < chunks.Length; i++)
            {
                NewMeshAPITest(i);
            }
            chunkTriangles.Dispose();
        }
        void OldMeshAPITest(int index, NativeArray<int> triChunk/*, NativeArray<float3> chunkF3Pos, NativeArray<Vector3> chunkV3Pos*/)
        {
            NativeArray<float3> chunkF3Pos = new NativeArray<float3>(sq(mapSettings.ChunkPointPerAxis), Allocator.Temp);
            NativeArray<Vector3> chunkV3Pos = new NativeArray<Vector3>(sq(mapSettings.ChunkPointPerAxis), Allocator.Temp);
            
            //float3[] chunkF3Pos = new float3[sq(mapSettings.ChunkPointPerAxis)];
            //Vector3[] chunkV3Pos = new Vector3[sq(mapSettings.ChunkPointPerAxis)];
            
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.name = $"ChunkMesh{chunks[index].Id}";
            chunkF3Pos.CopyFrom(FromJson<float3>(dir.GetChunkFileAt(chunks[index].Position, (int)ChunkFiles.VerticesPos)));
            //chunkV3Pos = FromJson<Vector3>(dir.GetChunkFileAt(chunks[0].Position, (int)ChunkFiles.VerticesPos));
            
            chunkV3Pos = chunkF3Pos.Reinterpret<Vector3>();
            //chunkF3Pos.CopyTo(chunkV3Pos);
            
            Debug.Log($"Vertice Number = {chunkV3Pos.Length} // num tri = {triChunk.Length}");
            mesh.SetVertices(chunkV3Pos);
            //mesh.triangles = chunkTriangles.ToArray();
            if (chunkV3Pos.Length == 36 && chunkTriangles.Length == 150)
            {
                //mesh.triangles = chunkTriangles.ToArray();
                mesh.SetIndices(chunkTriangles, MeshTopology.Triangles, 0, true);
                chunks[index].GetComponent<MeshFilter>().sharedMesh = mesh;
                chunks[index].GetComponent<MeshCollider>().sharedMesh = mesh;
            }
            
            chunkV3Pos.Dispose();
            chunkF3Pos.Dispose();
        }
            
        void NewMeshAPITest(int index)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.name = $"ChunkMesh{chunks[index].Id}";
                
            verticesPos = new NativeArray<float3>(sq(mapSettings.ChunkPointPerAxis), Allocator.Temp);
            verticesPos.CopyFrom(FromJson<float3>(dir.GetChunkFileAt(chunks[index].Position, (int)ChunkFiles.VerticesPos)));
            
            uvs = new NativeArray<float2>(sq(mapSettings.ChunkPointPerAxis), Allocator.Temp);
            uvs.CopyFrom(FromJson<float2>(dir.GetChunkFileAt(chunks[index].Position, (int)ChunkFiles.Uvs)));
            
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
            chunks[index].GetComponent<MeshFilter>().sharedMesh = mesh;
            chunks[index].GetComponent<MeshCollider>().sharedMesh = mesh;

            uvs.Dispose();
            verticesPos.Dispose();
        }
    }
}
