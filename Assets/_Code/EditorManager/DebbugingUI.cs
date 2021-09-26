using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.KwDelaunay;
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
        private DelaunayMethods delaunayMethods;
        
        [SerializeField] bool drawTriangle = false;
        [SerializeField] bool drawVoronoi = false;
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
            delaunayMethods = new DelaunayMethods(dir.SelectedSave);

/*
            foreach (var voronoiCell in delaunayMethods.GetVoronoiCells())
            {
                for (int i = 0; i < voronoiCell.Points.Length; i++)
                {
                    Debug.Log($"voronoiCell{i} = {voronoiCell.Points[i]}");
                }
                
            }
            
            
            List<int> t = new List<int>();
            t.AddRange(delaunayMethods.EdgesAroundPoint(3));
            for (int i = 0; i < t.Count; i++)
            {
                Debug.Log($"edges around? = {t[i]}");
            }
            
            foreach (IEdge edge in delaunayMethods.GetEdges())
            {
                for (int i = 0; i < t.Count; i++)
                {
                    if (edge.Index == t[i] || edge.Index == 3 || edge.Index == 30 || edge.Index == 25)
                    {
                        Debug.Log($"edges ID = {edge.Index}");
                        Debug.Log($"edge{i} = {t[i]} // point = P:{edge.P} / Q:{edge.Q}");
                    }
                }
            }
            */
            
            
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
            Gizmos.color = Color.blue;
            if (drawTriangle && !(delaunayMethods is null))
            {
                delaunayMethods.ForEachTriangleEdge(edge =>
                {
                    if (edge.Index == 3)
                    {
                        Gizmos.color = Color.magenta;
                    }
                    else
                    {
                        Gizmos.color = Color.blue;
                    }
                    Gizmos.DrawLine((edge.P), (edge.Q));
                });
            }

            if (drawVoronoi && !(delaunayMethods is null))
            {
                delaunayMethods.ForEachVoronoiEdge(cell =>
                {
                    if (cell.Index != -1)
                    {
                        Gizmos.DrawLine((cell.P), (cell.Q));
                    }
                });
            }
        }
    }
}
