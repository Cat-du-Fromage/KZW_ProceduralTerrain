using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;
using static Unity.Mathematics.math;
using int2 = Unity.Mathematics.int2;

namespace KaizerWaldCode
{
	public class HashVisualization : MonoBehaviour
	{
		static int hashesId = Shader.PropertyToID("_Hashes");
		static int	configId = Shader.PropertyToID("_Config");


		[SerializeField] Mesh instanceMesh;
		[SerializeField] Material material;
		[SerializeField, Range(1, 512)] int resolution = 16;
		[SerializeField] private int seed = 0;
		[SerializeField, Range(-2f, 2f)] private float verticalOffset = 1f;
/*
		[SerializeField] int2 textureSize;
		
		[SerializeField] Renderer textureRender;
		[SerializeField] MeshFilter meshFilter;
		[SerializeField] MeshRenderer meshRenderer;

		[SerializeField] Texture2D tex;
*/
		NativeArray<uint> hashes;

		ComputeBuffer hashesBuffer;

		MaterialPropertyBlock propertyBlock;
		
		void OnEnable() 
		{
			int length = resolution * resolution;
			hashes = new NativeArray<uint>(length, Allocator.Persistent);
			hashesBuffer = new ComputeBuffer(length, 4);

			
			new HashJob 
			{
				JHashes = hashes
			}.ScheduleParallel(hashes.Length, resolution, default).Complete();

			hashesBuffer.SetData(hashes);

			propertyBlock ??= new MaterialPropertyBlock();
			propertyBlock.SetBuffer(hashesId, hashesBuffer);
			propertyBlock.SetVector(configId, new Vector4(resolution, 1f / resolution, verticalOffset / resolution));
		}
		void OnDisable() 
		{
			if(hashes.IsCreated)hashes.Dispose();
			hashesBuffer.Release();
			hashesBuffer = null;
		}

		void OnValidate()
		{
			//TextureSizeUpdate();

			if (!(hashesBuffer is null) && enabled) 
			{
				OnDisable();
				OnEnable();
			}
		}
		
		void Update () 
		{
			Graphics.DrawMeshInstancedProcedural
			(
				instanceMesh, 0, material, new Bounds(Vector3.zero, Vector3.one),
				hashes.Length, propertyBlock
			);
		}
		
		[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
		private struct HashJob : IJobFor
		{
			[WriteOnly] public NativeArray<uint> JHashes;
        
			public int Resolution;

			public float InvResolution;

			public SmallXXHash Hash;

			public void Execute(int i)
			{
				var v = (int)floor(InvResolution * i + 0.00001f);
				var u = i - Resolution * v - Resolution / 2;
				v -= Resolution / 2;

				JHashes[i] = Hash.Eat(u).Eat(v);
			}
		}

/*
		[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
		struct HashJob : IJobFor 
		{
			[WriteOnly]
			public NativeArray<uint> hashes;
	        
			public void Execute(int i) 
			{
				hashes[i] = (uint)i;
			}
		}
		*/
		/*
		Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
		{
			Texture2D texture = new Texture2D(width, height);
			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.SetPixels(colourMap);
			texture.Apply();
			return texture;
		}
		*/
		/*
		[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
		struct ConfigureProcedural  : IJobFor
		{
			
			[ReadOnly] public float4 JConfig;
			[ReadOnly] public NativeArray<uint> JHashes;
			[WriteOnly] public NativeArray<Color> JColors;
			
	        
			public void Execute(int index) 
			{
				float v = floor(JConfig.y * index);
				float u = index - JConfig.x * v;
				
				uint hash = JHashes[index];
		
				float4x4 ObjectToWorld = Matrix4x4.zero;
				ObjectToWorld.c0.w = JConfig.y * (u + 0.5f) - 0.5f;
				ObjectToWorld.c1.w = 0.0f;
				ObjectToWorld.c2.w = JConfig.y * (v + 0.5f) - 0.5f;
				ObjectToWorld.c3.w = 1.0f;

				ObjectToWorld.c0.x = JConfig.y;
				ObjectToWorld.c1.y = JConfig.y;
				ObjectToWorld.c2.z = JConfig.y;
			}
			
			float3 GetHashColor (int i) 
			{
				uint hash = JHashes[i];
				float3 baseColor = float3(JConfig.y * JConfig.y * hash);
				return  baseColor;
			}
		}
		*/
	}
}
