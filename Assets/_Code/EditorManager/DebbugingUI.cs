using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.TerrainGeneration;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.TerrainGeneration.KwSystem;
using KaizerWaldCode.Utils;
using Unity.Mathematics;
using UnityEngine;

using static KaizerWaldCode.Utils.KWmath;

namespace KaizerWaldCode
{
    public class DebbugingUI : MonoBehaviour
    {
        
        public bool PoissonDiscDebug;
        private bool debugEnable;
        private bool arrayInit;
        private MapSettingsData mapSettings;
        private MapDirectories dir;
        private float3[] pdcs;
        private int[] island;
        
        private void Awake()
        {
            debugEnable = false;
            arrayInit = false;
        }

        public void DebuggingEnable(SettingsData mapset, in string selectedSave)
        {
            mapSettings = mapset;
            debugEnable = true;
            dir.SelectedSave = selectedSave;
            pdcs = new float3[sq(mapSettings.NumCellMap)];
            island = new int[sq(mapSettings.NumCellMap)];
            InitPoissonArray();
        }

        void InitPoissonArray()
        {
            if (PoissonDiscDebug)
            {
                if (JsonHelper.FromJson<float3>(dir.GetFullMapFileAt((int)FullMapFiles.PoissonDiscPos)).Length == sq(mapSettings.NumCellMap))
                {
                    pdcs = new float3[sq(mapSettings.NumCellMap)];
                    island = new int[sq(mapSettings.NumCellMap)];
                    pdcs = JsonHelper.FromJson<float3>(dir.GetFullMapFileAt((int) FullMapFiles.PoissonDiscPos));
                    island = JsonHelper.FromJson<int>(dir.GetFullMapFileAt((int) FullMapFiles.Island));
                }
            }
        }
        
        private void OnValidate()
        {
            if (!debugEnable) return;
            InitPoissonArray();
        }
        
        
        void OnDrawGizmos()
        {
            if (!debugEnable || !PoissonDiscDebug) return;
            if (pdcs.Length != 0)
            {
                for (int i = 0; i < pdcs.Length; i++)
                {
                    Gizmos.color = Color.blue;
                    if (island[i] == 1)
                    {
                        Gizmos.color = Color.green;
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                    }
                    Gizmos.DrawSphere(pdcs[i], 0.1f);
                }
            }
        }
    }
}
