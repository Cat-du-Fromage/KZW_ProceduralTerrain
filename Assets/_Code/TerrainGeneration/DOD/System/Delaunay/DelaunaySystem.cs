using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.TerrainGeneration.KwSystem;
using KaizerWaldCode.Utils;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.Utils.KWmath;

using float2 = Unity.Mathematics.float2;
using float3 = Unity.Mathematics.float3;

namespace KaizerWaldCode.TerrainGeneration
{
    
    public partial class DelaunaySystem
    {
        private readonly int[] EDGE_STACK = new int[512];

        //public int[] Triangles { get; private set; }
        //public int[] Halfedges { get; private set; }
        //public IPoint[] Points { get; private set; }
/*
        private readonly int hashSize;
        private readonly int[] hullPrev;
        private readonly int[] hullNext;
        private readonly int[] hullTri;
        private readonly int[] hullHash;

        private double cx;
        private double cy;

        private int trianglesLen;
        private readonly int hullStart;
        private readonly int hullSize;
        private readonly int[] hull;
*/
        public DelaunaySystem(ref NativeArray<int> poissonDisc, in string savePath, in MapSettingsData set)
        {
            if (poissonDisc.Length < 3) return;
            #region Init Coords

            MapDirectories dir = new MapDirectories();
            dir.SelectedSave = savePath;
            
            //Get Poisson Disc Pos From Json
            NativeArray<float3> coords = AllocNtvAry<float3>(sq(set.NumCellMap));
            coords.CopyFrom(JsonHelper.FromJson<float3>(dir.GetFullMapFileAt((int)FullMapFiles.PoissonDiscPos)));

            #endregion Init Coords

            int n = coords.Length;
            int maxTriangles = mad(2, n, -5);

            float2 center = float2(set.MapSize * 0.5f);// CAREFULL Try with float2.zero if there is a bug
            
            int centerCellId = FindCell(center, set.NumCellMap, set.CellSize);
            
            //Get Ids from save files
            //maybe not needed since its only needed to get nearest point of circumcenter later
            NativeArray<int> poissonDiscIds = AllocNtvAry<int>(n);
            poissonDiscIds.CopyFrom(JsonHelper.FromJson<int>(dir.GetFullMapFileAt((int)FullMapFiles.PoissonDiscId)));
            
            int i0 = 0;
            int i1 = 0;
            int i2 = 0;

            float3 i0Pos = float3.zero;
            float3 i1Pos = float3.zero;
            float3 i2Pos = float3.zero;

            (i0, i0Pos) = InitI0(coords, centerCellId);
            Debug.Log($"i0 {i0} ; i0Pos = {i0Pos}");

            (i1, i1Pos) = InitI1(i0, i0Pos, coords, set.NumCellMap);
            Debug.Log($"i1 {i1} ; i1Pos = {i1Pos}");

            // i2 Calcul
            float minRadius = float.MaxValue;
            // find the third point which forms the smallest circumcircle with the first two
            for (int i = 0; i < n; i++)
            {
                if (i == i0 || i == i1) continue;
                float r = Circumradius(i0Pos.xz, i1Pos.xz, coords[i].xz);
                if (r < minRadius)
                {
                    i2 = i;
                    minRadius = r;
                }
            }

            i2Pos = coords[i2];

            //can't return a single scalar from Job so...
            NativeArray<int> i2Arr = new NativeArray<int>(1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<float3> i2PosArr = new NativeArray<float3>(1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            
            i2 = i2Arr[0];
            i2Pos = i2PosArr[0];
            i2Arr.Dispose();
            i2PosArr.Dispose();
            //Debug.Log($"i0p = {i0Pos}; i1p = {i1Pos}; i2p = {i2Pos}");
            //Debug.Log($"i0 = {i0}; i1 = {i1}; i2 = {i2}");
            if (IsLeft(i0Pos.xz, i1Pos.xz, i2Pos.xz))
            {
                int i = i1;
                float x = i1Pos.x;
                float y = i1Pos.y;
                i1 = i2;
                i1Pos.x = i2Pos.x;
                i1Pos.y = i2Pos.y;
                i2 = i;
                i2Pos.x = x;
                i2Pos.y = y;
            }

            //original :
            //float2 circumCenter = Circumcenter(i0x, i0y, i1x, i1y, i2x, i2y);
            //center = circumCenter;
            center = GetCircumcenter(i0Pos.xz, i1Pos.xz, i2Pos.xz);
        }
    }
    
}
