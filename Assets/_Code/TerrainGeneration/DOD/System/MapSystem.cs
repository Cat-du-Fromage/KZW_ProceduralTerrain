using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KaizerWaldCode.KWSerialization;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.TerrainGeneration.KwEntity;
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
        private string[] paths;

        private NativeArray<float3> verticesPos;
        private NativeArray<int> verticesCellIndex;

        public void LoadMap(in SettingsData mapSet, in bool newGame, in string folderName = "default")
        {
            bitfield = new BitField32(uint.MaxValue);
            mapSettings = mapSet;
            mapPointSurface = sq(mapSet.MapPointPerAxis);

            paths = new string[]
            {
                $"{Application.persistentDataPath}/Save Files/{folderName}/VerticesPosition.txt",
                $"{Application.persistentDataPath}/Save Files/{folderName}/VerticesCellIndex.txt",
                /*
                $"{Application.persistentDataPath}/Save Files/{folderPath}/PoissonDiscPosition.txt",
                $"{Application.persistentDataPath}/Save Files/{folderPath}/PoissonDiscCellIndex.txt",
                $"{Application.persistentDataPath}/Save Files/{folderPath}/Voronoi.txt",
                $"{Application.persistentDataPath}/Save Files/{folderPath}/IslandShape.txt",
                $"{Application.persistentDataPath}/Save Files/{folderPath}/Noise.txt",
                $"{Application.persistentDataPath}/Save Files/{folderPath}/FallOff.txt",
                */
            };

            string fullPath = $"{Application.persistentDataPath}/Save Files/{folderName}";

            if (newGame)
            {
                LoadNewMap(fullPath);
            }
            else
            {
                LoadSavedMap(fullPath);
            }

        }

        public void LoadSavedMap(in string directoryPath = "default")
        {
        }

        public void LoadNewMap(in string directoryPath = "default")
        {
            if (!Directory.Exists(directoryPath)) {Directory.CreateDirectory(directoryPath);}

            bitfield.SetBits(0,false, paths.Length);

            for (int i = 0; i < paths.Length; i++)
            {
                if (!SaveExist(paths[i])) { CreateCloseFile(paths[i]); }
                StateMachineMap(i);
                bitfield.SetBits(i, true);
            }
            
        }

        void FillGap()
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (!bitfield.IsSet(i))
                {
                    StateMachineMap(i);
                    bitfield.SetBits(i, true);
                }
            }
        }

        void NewGameProcess()
        {
            JobHandle generalDependency = new JobHandle();
            JobHandle newGameDependency = new JobHandle();
            verticesPos = AllocNtvAry<float3>(mapPointSurface);
            verticesCellIndex = AllocFillNtvAry<int>(mapPointSurface, -1);
            newGameDependency = VerticesDoubleProcess(generalDependency);
            newGameDependency.Complete();
            Save(paths[0], verticesPos.ToArray());
            Save(paths[1], verticesCellIndex.ToArray());
            verticesPos.Dispose();
            verticesCellIndex.Dispose();
        }

        void StateMachineMap(in int state)
        {
            JobHandle generalDependency = new JobHandle();
            switch (state)
            {
                case 0:
                    verticesPos = AllocNtvAry<float3>(mapPointSurface);

                    VerticesPositionProcess(generalDependency);
                    Save(paths[state], verticesPos.ToArray());

                    verticesPos.Dispose();
                    break;
                case 1:
                    verticesCellIndex = AllocFillNtvAry<int>(mapPointSurface, -1);
                    verticesPos = AllocNtvAry<float3>(mapPointSurface);
                    verticesPos.CopyFrom(Load<float3>(paths[0]));

                    VerticesCellIndexProcess(generalDependency);
                    Save(paths[state], verticesCellIndex.ToArray());

                    verticesPos.Dispose();
                    verticesCellIndex.Dispose();
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
            }
        }

        void OnDestroy()
        {
            if (verticesPos.IsCreated) verticesPos.Dispose();
            if (verticesCellIndex.IsCreated) verticesCellIndex.Dispose();
        }

        /*
        // Update is called once per frame
        void Update()
        {
            if (bitfield.TestAll(0, 16) || paths.Length < 1) return;
            FillGap();
        }
        */
    }
}
