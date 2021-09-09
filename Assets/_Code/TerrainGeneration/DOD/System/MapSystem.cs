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

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    public partial class MapSystem : MonoBehaviour
    {
        private SettingsData mapSettings;

        private BitField32 bitfield;
        private string[] paths;

        public void Initialize(SettingsData mapSet, in string folderName = "default")
        {
            mapSettings = mapSet;
            bitfield = new BitField32(uint.MaxValue);

            string fullPath = $"{Application.persistentDataPath}/Saves Files/{folderName}";
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

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
            bitfield.SetBits(0,false, paths.Length);

            for (int i = 0; i < paths.Length; i++)
            {
                Debug.Log(paths[i].ToString());

                if (!BinarySerialization.SaveExist(paths[i]))
                {
                    File.Create(paths[i]);
                    StateMachineMap(i);
                    bitfield.SetBits(i, true);
                }
                else
                {
                    StateMachineMap(i);
                    bitfield.SetBits(i, true);
                }
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

        void StateMachineMap(int state)
        {
            JobHandle Dependency = new JobHandle();
            bitfield.SetBits(state, true);
            switch (state)
            {
                case 0:
                    Debug.Log("Pos trying");
                    NativeArray<float3> pos = AllocNtvAry<float3>(sq(mapSettings.MapPointPerAxis));
                    NativeArray<int> id = AllocNtvAry<int>(sq(mapSettings.MapPointPerAxis));
                    JobHandle vDependency = VerticesProcess(Dependency, pos, id, mapSettings);
                    vDependency.Complete();
                    BinarySerialization.Save(paths[state], pos.ToArray());
                    BinarySerialization.Save(paths[1], id.ToArray());
                    pos.Dispose();
                    id.Dispose();
                    break;
                case 1:
                    //int[] test = BinarySerialization.Load<int>(paths[1]);
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

        // Update is called once per frame
        void Update()
        {
            if (bitfield.TestAll(0, 16) || paths.Length < 1) return;
            FillGap();
        }
    }
}
