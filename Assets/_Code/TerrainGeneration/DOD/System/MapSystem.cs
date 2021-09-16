using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KaizerWaldCode.TerrainGeneration.Data;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.KWSerialization.BinarySerialization;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    public partial class MapSystem : MonoBehaviour
    {

        private SettingsData mapSettings;
        private int mapPointSurface;

        private BitField32 bitfield;

        private GameObject[] chunks;

        private NativeArray<float3> verticesPos; // raw position calcul
        private NativeArray<int> verticesCellIndex; // raw position calcul

        private NativeArray<float3> sortedVerticesPos; // use for chunkSlice
        private NativeArray<int> sortedVerticesCellIndex; // use for chunkSlice

        private JobHandle gDependency; // needed for jobs system

        public void LoadMap(in SettingsData mapSet, in bool newGame, in string folderName = "default")
        {
            bitfield = new BitField32(uint.MaxValue);
            mapSettings = mapSet;
            mapPointSurface = sq(mapSet.MapPointPerAxis);

            GetOrCreateDirectories(folderName);
            if (newGame)
            {
                LoadNewMap();
            }
            else
            {
                LoadSavedMap();
            }

        }

        public void LoadSavedMap()
        {
        }

        public void LoadNewMap()
        {
            bitfield.SetBits(0,false, dir.FullMapFilesPath.Length); //Full Map
            //bitfield.SetBits(16, false, 2); //Chunk Slice

            for (int i = 0; i < dir.FullMapFilesPath.Length; i++)
            {
                if (bitfield.IsSet(i)) {continue;}
                if ( !SaveExist(dir.GetFullMapFileAt(i)) ) { CreateFile(dir.GetFullMapFileAt(i)); }
                StateMachineMap(i);
                bitfield.SetBits(i, true);
            }
            
        }

        void StateMachineMap(in int state)
        {
            switch (state)
            {
                case 0: 
                    VerticesPositionProcess(gDependency);
                    break;
                case 1:
                    VerticesCellIndexProcess(gDependency);
                    PoissonDiscProcess(gDependency);
                    CreateChunkProcess();
                    VerticesSliceProcess();
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:
                    break;
                default:
                    break;

                    //CreateChunkProcess();
                    //VerticesSliceProcess();
            }
        }
        void OnDestroy()
        {
            if (verticesPos.IsCreated) verticesPos.Dispose();
            if (verticesCellIndex.IsCreated) verticesCellIndex.Dispose();
            if (sortedVerticesPos.IsCreated) sortedVerticesPos.Dispose();
            if (sortedVerticesCellIndex.IsCreated) sortedVerticesCellIndex.Dispose();
        }
        
        void OnDrawGizmos()
        {
            if (pdcs.Length != 0)
            {
                Debug.Log("ok i suppose?");
                for (int i = 0; i < pdcs.Length; i++)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(pdcs[i], 0.1f);
                }
            }
        }
    }
}
