using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.KWSerialization;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.TerrainGeneration.KwEntity;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    /*
    struct MapDatasFilePath
    {
        const string SaveFilePath = "Saves Files";
        public string folderPath;

        public MapDatasFilePath(string s)
        {
            folderPath = s;
        }


        public readonly string GetFullPath(string s)
        {
            return $"{Application.persistentDataPath}/Saves Files/{s}";
        }
    }
    */
    public class MapSystem : MonoBehaviour
    {
        private BitField32 bitfield;

        //CAREFUL the Folder may not be the same
        private string[] paths;
        //private List<Action> method;

        void Initialize(in string folderPath = "default")
        {
            bitfield = new BitField32();
            paths = new string[8]
            {
                $"{Application.persistentDataPath}/Saves Files/{folderPath}/VerticesPosition.txt",
                $"{Application.persistentDataPath}/Saves Files/{folderPath}/VerticesCellIndex.txt",
                $"{Application.persistentDataPath}/Saves Files/{folderPath}/PoissonDiscPosition.txt",
                $"{Application.persistentDataPath}/Saves Files/{folderPath}/PoissonDiscCellIndex.txt",
                $"{Application.persistentDataPath}/Saves Files/{folderPath}/Voronoi.txt",
                $"{Application.persistentDataPath}/Saves Files/{folderPath}/IslandShape.txt",
                $"{Application.persistentDataPath}/Saves Files/{folderPath}/Noise.txt",
                $"{Application.persistentDataPath}/Saves Files/{folderPath}/FallOff.txt",
            };

            for (int i = 0; i < paths.Length; i++)
            {
                if (BinarySerialization.SaveExist(paths[i]))
                    bitfield.SetBits(i, true);
                else
                    bitfield.SetBits(i, false);
            }
        }

        void FillGap()
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (!bitfield.IsSet(i))
                {
                    //method[i].Invoke();
                    StateMachineMap(i);
                    bitfield.SetBits(i, true);
                }
            }
        }

        void StateMachineMap(int state)
        {
            bitfield.SetBits(state, true);
            switch (state)
            {
                case 1 << 0:
                    break;
                case 1 << 1:
                    break;
                case 1 << 2:
                    break;
                case 1 << 3:
                    break;
                case 1 << 4:
                    break;
                case 1 << 5:
                    break;
                case 1 << 6:
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
