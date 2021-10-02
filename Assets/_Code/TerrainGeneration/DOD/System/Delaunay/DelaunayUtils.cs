using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using KaizerWaldCode.Utils;
using KaizerWaldCode.TerrainGeneration;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.KwDelaunay.DelaunayI2Utils;

using int2 = Unity.Mathematics.int2;
using float2 = Unity.Mathematics.float2;
using float3 = Unity.Mathematics.float3;

namespace KaizerWaldCode.TerrainGeneration.KwDelaunay
{
    public partial class DelaunaySystem
    {
        /// <summary>
        /// Return the position(float2) of the point in the center of the map (imply the grid is ordered)
        /// </summary>
        /// <param name="samplesCellGrid"></param>
        /// <param name="cellGridCenter"></param>
        /// <param name="numCellJob"></param>
        /// <returns></returns>
        private (int,float3) InitI0(in NativeArray<float3> pDiscCellGrid, in int cellGridCenter)
        {
            int i0 = 0;
            float3 i0Pos = float3.zero;
            
            i0 = cellGridCenter;
            i0Pos = pDiscCellGrid[cellGridCenter];
            return (i0, i0Pos);
        }

        /// <summary>
        /// Find the point closest to the seed
        /// </summary>
        /// <param name="i0Id"></param>
        /// <param name="i0Pos"></param>
        /// <param name="samplesCellGrid"></param>
        /// <param name="mapNumCell"></param>
        /// <returns></returns>
        private (int, float3) InitI1(in int i0Id, in float3 i0Pos, in NativeArray<float3> samplesCellGrid, in int mapNumCell)
        {
            int i1 = 0;
            float3 i1Pos = float3.zero;
            int2 xRange;
            int2 yRange;
            int numCell;

            CellGridRanges(mapNumCell, i0Id, out xRange, out yRange, out numCell);

            NativeArray<int> cellsIndex = new NativeArray<int>(numCell, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            NativeArray<float> distances = new NativeArray<float>(numCell, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            int cellCount = 0;
            for (int y = yRange.x; y <= yRange.y; y++)
            {
                for (int x = xRange.x; x <= xRange.y; x++)
                {
                    int indexCellOffset = i0Id + mad(y, mapNumCell, x);
                    cellsIndex[cellCount] = indexCellOffset;
                    cellCount++;
                }
            }
            
            for (int i = 0; i < numCell; i++)
            {
                distances[i] = select(distancesq(i0Pos, samplesCellGrid[cellsIndex[i]]), float.MaxValue, samplesCellGrid[cellsIndex[i]].Equals(i0Pos));
            }

            int minDstId = KwGrid.IndexMin(distances, cellsIndex);
            i1 = minDstId;
            i1Pos = samplesCellGrid[minDstId];
            
            cellsIndex.Dispose();
            distances.Dispose();
            return (i1, i1Pos);
        }
        
        /// <summary>
        /// 1) Get cell to check circumradius Distance
        /// 2) Calcul Each Circumradius
        /// 3) sort and find the nearest point I2
        /// </summary>
        /// <param name="i0Id"></param>
        /// <param name="i1Id"></param>
        /// <param name="mapNumCell"></param>
        /// <param name="sCG">sampleCellGrid</param>
        private (int, float3) InitI2(in int i0Id, in int i1Id, in int mapNumCell, in NativeArray<float3> sCG)
        {
            int i2Id = 0;

            //Get the I who has the lowest index
            int iLow = (i0Id > i1Id) ? i0Id : i1Id;
            //Assign iUp to the other
            int iUp = (iLow == i0Id) ? i1Id : i0Id;

            int2 iLowXY = KwGrid.GetXY2(iLow, mapNumCell);
            int2 iUpXY = KwGrid.GetXY2(iUp, mapNumCell);
            
            // find the third point which forms the smallest circumcircle with the first two
            using NativeList<int> cellToCheck = new NativeList<int>(4, Allocator.Temp);
            using NativeArray<int> lowIds = GetLowGridIDS(mapNumCell, iLow, iLowXY);
            using NativeArray<int> upIds = GetUpGridIDS(mapNumCell, iUp, iUpXY);
            
            //merge low and up cells (get also rid of duplicates)
            cellToCheck.AddRange(lowIds);
            for (int i = 0; i < upIds.Length; i++)
            {
                if (cellToCheck.Contains(upIds[i])) continue; //skip duplicate
                cellToCheck.Add(upIds[i]);
            }
            i2Id = IndexMinCircumRad(iLow, iUp, mapNumCell, sCG, cellToCheck.AsArray());
            return (i2Id, sCG[i2Id]); //i2Pos = sampleCellGrid[i2Id]
        }
        /// <summary>
        /// Sort all poissonDisc relative to their distances from the circumcenter of the triangle previously calculated
        /// </summary>
        /// <param name="center"></param>
        /// <param name="coords"></param>
        /// <param name="unsortedDst"></param>
        /// <returns></returns>
        private JobHandle GetSortedDstCoords(in float2 center, in NativeArray<float3> coords, ref NativeArray<float> unsortedDst)
        {
            int sizeArray = coords.Length;

            DstPointsToCircumCenterJob unsortedDstJ = new DstPointsToCircumCenterJob
            {
                JCircumcenterPos = center,
                JPoissonPos = coords,
                JDst = unsortedDst
            };
            JobHandle unsortedDstJH = unsortedDstJ.ScheduleParallel(sizeArray, JobsUtility.JobWorkerCount - 1, new JobHandle());
            return unsortedDstJH;
        }

        /// <summary>
        /// Get the index of the Min circumradius
        /// </summary>
        /// <param name="index"></param>
        /// <param name="mapNumCell"></param>
        /// <param name="checkRange"></param>
        /// <param name="sCG"></param>
        /// <returns></returns>
        private int IndexMinCircumRad(in int i0ID, in int i1ID, in int mapNumCell, in NativeArray<float3> scg, in NativeArray<int> ctc)
        {
            int minCircumId = 0;
            float minRadius = float.PositiveInfinity;
            for (int i = 0; i < ctc.Length; i++)
            {
                int checkIndex = ctc[i];
                float r = Circumradius(scg[i0ID].xz, scg[i1ID].xz, scg[checkIndex].xz);
                if (r < minRadius)
                {
                    minCircumId = checkIndex;
                    minRadius = r;
                }
            }
            return minCircumId;
        }
        
        /// <summary>
        /// Get both X/Y grid Range (neighbores around the cell)
        /// Get numCell to check (may be less if the cell checked is on a corner or on an edge of the grid)
        /// </summary>
        /// <param name="cell">index of the current cell checked</param>
        /// <param name="xRange"></param>
        /// <param name="yRange"></param>
        /// <param name="numCell"></param>
        private void CellGridRanges(in int mapNumCell, in int cell, out int2 xRange, out int2 yRange, out int numCell)
        {
            int x, y;
            (x, y) = KwGrid.GetXY(cell, mapNumCell);
            //int y = (int)floor((float)cell / mapNumCell);
            //int x = cell - (y * mapNumCell);

            bool corner = (x == 0 & y == 0) || (x == 0 & y == mapNumCell - 1) || (x == mapNumCell - 1 & y == 0) || (x == mapNumCell - 1 & y == mapNumCell - 1);
            bool yOnEdge = y == 0 | y == mapNumCell - 1;
            bool xOnEdge = x == 0 | x == mapNumCell - 1;

            //check if on edge 0 : int2(0, 1) ; if not NumCellJob - 1 : int2(-1, 0)
            int2 OnEdge(int e) => select(int2(-1, 0), int2(0, 1), e == 0);
            yRange = select(OnEdge(y), int2(-1, 1), !yOnEdge);
            xRange = select(OnEdge(x), int2(-1, 1), !xOnEdge);
            numCell = select(select(9, 6, yOnEdge || xOnEdge), 4, corner);
        }

        private int HashKey(in float2 h)
        {
           return (int)floor(PseudoAngle(float2(h.x - center.x, h.y - center.y) * hashSize) % hashSize);
        } 
        private float PseudoAngle(in float2 d)
        {
            float p = d.x / (abs(d.x) + abs(d.y));
            return (d.y > 0 ? 3 - p : 1 + p) / 4; // [0..1]
        }
        private int AddTriangle(in int i0, in int i1, in int i2, in int a, in int b, in int c)
        {
            int t = trianglesLen;

            triangles[t] = i0;
            triangles[t + 1] = i1;
            triangles[t + 2] = i2;

            Link(t, a);
            Link(t + 1, b);
            Link(t + 2, c);

            trianglesLen += 3;
            return t;
        }
        private void Link(in int a, in int b)
        {
            Halfedges[a] = b;
            if (b != -1) Halfedges[b] = a;
        }
        private void Swap(ref NativeArray<int> ids, in int i, in int j)
        {
            (ids[i], ids[j]) = (ids[j], ids[i]); // new way of swap in C#
        }
        private void Quicksort(ref NativeArray<int> ids, in NativeArray<float> dists, int left, int right)
        {
            if (right - left <= 20)
            {
                for (var i = left + 1; i <= right; i++)
                {
                    int temp = ids[i];
                    float tempDist = dists[temp];
                    int j = i - 1;
                    while (j >= left && dists[ids[j]] > tempDist) ids[j + 1] = ids[j--];
                    ids[j + 1] = temp;
                }
            }
            else
            {
                int median = (left + right) >> 1;
                int i = left + 1;
                int j = right;
                Swap(ref ids, median, i);
                if (dists[ids[left]] > dists[ids[right]]) Swap(ref ids, left, right);
                if (dists[ids[i]] > dists[ids[right]]) Swap(ref ids, i, right);
                if (dists[ids[left]] > dists[ids[i]]) Swap(ref ids, left, i);

                int temp = ids[i];
                float tempDist = dists[temp];
                while (true)
                {
                    do i++; while (dists[ids[i]] < tempDist);
                    do j--; while (dists[ids[j]] > tempDist);
                    if (j < i) break;
                    Swap(ref ids, i, j);
                }
                ids[left + 1] = ids[j];
                ids[j] = temp;

                if (right - i + 1 >= j - left)
                {
                    Quicksort(ref ids, dists, i, right);
                    Quicksort(ref ids, dists, left, j - 1);
                }
                else
                {
                    Quicksort(ref ids, dists, left, j - 1);
                    Quicksort(ref ids, dists, i, right);
                }
            }
        }
    }
}
