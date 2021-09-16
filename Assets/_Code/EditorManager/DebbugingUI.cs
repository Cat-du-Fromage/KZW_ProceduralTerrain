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
        private bool MapInit;
        public bool PoissonDiscDebug;
        private MapSettingsData mapSettings;
        private MapDirectories dir;
        private float3[] pdcs;

        public void DebuggingOK(SettingsData mapset, in string selectedSave)
        {
            mapSettings = mapset;
            MapInit = true;
            dir.SelectedSave = selectedSave;
            Debug.Log(dir.GetFullMapFileAt((int)FullMapFiles.PoissonDiscPos));
            pdcs = new float3[sq(mapSettings.NumCellMap)];
            pdcs = JsonHelper.FromJson<float3>(dir.GetFullMapFileAt((int) FullMapFiles.PoissonDiscPos));
        }

        private void Awake()
        {
            MapInit = false;
        }

        private void OnValidate()
        {
            if (!MapInit) return;
            if (PoissonDiscDebug)
            {
                pdcs = new float3[sq(mapSettings.NumCellMap)];
                pdcs = JsonHelper.FromJson<float3>(dir.GetFullMapFileAt((int) FullMapFiles.PoissonDiscPos));
            }
        }
        
        
        void OnDrawGizmos()
        {
            if (!MapInit) return;
            if (pdcs.Length != 0)
            {
                for (int i = 0; i < pdcs.Length; i++)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(pdcs[i], 0.1f);
                }
            }
        }
    }
}
