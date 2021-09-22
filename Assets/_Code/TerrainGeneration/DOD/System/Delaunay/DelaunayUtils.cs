using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.Utils.KWmath;

using float2 = Unity.Mathematics.float2;
using float3 = Unity.Mathematics.float3;

namespace KaizerWaldCode.TerrainGeneration
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

            int minDstId = IndexMin(distances, cellsIndex);
            i1 = minDstId;
            i1Pos = samplesCellGrid[minDstId];
            
            cellsIndex.Dispose();
            distances.Dispose();
            return (i1, i1Pos);
        }

        private (int, float2) InitI2(int i0Id, float2 i0Pos, int i1Id, float2 i1Pos, NativeArray<float2> samplesCellGrid, int mapNumCell)
        {
            int i2 = 0;
            float2 i2Pos = float2.zero;
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

            bool NotValid(int a) => samplesCellGrid[cellsIndex[a]].Equals(float2(-1)) || samplesCellGrid[cellsIndex[a]].Equals(i0Pos);
            for (int i = 0; i < numCell; i++)
            {
                distances[i] = select(distancesq(i0Pos, samplesCellGrid[cellsIndex[i]]), float.MaxValue, NotValid(i));
            }

            int minDstId = IndexMin(distances, cellsIndex);
            i2 = minDstId;
            i2Pos = samplesCellGrid[minDstId];

            cellsIndex.Dispose();
            distances.Dispose();
            return (i2, i2Pos);
        }


        /// <summary>
        /// Find the index of the cells a point belongs to
        /// </summary>
        /// <param name="pointPos">point from where we want to find the cell</param>
        /// <param name="numCellMap">number of cells per axis (fullmap : mapSize * numChunk / radius)</param>
        /// <param name="cellRadius">radius on map settings</param>
        /// <returns>index of the cell</returns>
        private int FindCell(float2 pointPos, int numCellMap, float cellRadius)
        {
            int2 cellGrid = int2(numCellMap);
            for (int i = 0; i < numCellMap; i++)
            {
                if (cellGrid.y == numCellMap) cellGrid.y = select(numCellMap, i, pointPos.y <= mad(i, cellRadius, cellRadius));
                if (cellGrid.x == numCellMap) cellGrid.x = select(numCellMap, i, pointPos.x <= mad(i, cellRadius, cellRadius));
                if (cellGrid.x != numCellMap && cellGrid.y != numCellMap) break;
            }
            return mad(cellGrid.y, numCellMap, cellGrid.x);
        }


        /// <summary>
        /// Get both X/Y grid Range (neighbores around the cell)
        /// Get numCell to check (may be less if the cell checked is on a corner or on an edge of the grid)
        /// </summary>
        /// <param name="cell">index of the current cell checked</param>
        /// <param name="xRange"></param>
        /// <param name="yRange"></param>
        /// <param name="numCell"></param>
        private void CellGridRanges(int mapNumCell, int cell, out int2 xRange, out int2 yRange, out int numCell)
        {
            int y = (int)floor(cell / (float)mapNumCell);
            int x = cell - mul(y, mapNumCell);

            bool corner = (x == 0 && y == 0) || (x == 0 && y == mapNumCell - 1) || (x == mapNumCell - 1 && y == 0) || (x == mapNumCell - 1 && y == mapNumCell - 1);
            bool yOnEdge = y == 0 || y == mapNumCell - 1;
            bool xOnEdge = x == 0 || x == mapNumCell - 1;

            //check if on edge 0 : int2(0, 1) ; if not NumCellJob - 1 : int2(-1, 0)
            int2 OnEdge(int e) => select(int2(-1, 0), int2(0, 1), e == 0);
            yRange = select(OnEdge(y), int2(-1, 1), !yOnEdge);
            xRange = select(OnEdge(x), int2(-1, 1), !xOnEdge);
            numCell = select(select(9, 6, yOnEdge || xOnEdge), 4, corner);
        }

        /// <summary>
        /// Find the index of the minimum value of the array
        /// </summary>
        /// <param name="dis">array containing float distance value from point to his neighbors</param>
        /// <param name="cellIndex">array storing index of float2 position of poissonDiscSamples </param>
        /// <returns>index of the closest point</returns>
        private int IndexMin(in NativeArray<float> dis, in NativeArray<int> cellIndex)
        {
            float val = float.MaxValue;
            int index = 0;

            for (int i = 0; i < dis.Length; i++)
            {
                if (dis[i] < val)
                {
                    index = cellIndex[i];
                    val = dis[i];
                }
            }
            return index;
        }
        
        /// <summary>
        /// Find who from i0 and i1 is the left or right one
        /// return also configuration
        /// </summary>
        /// <param name="lr">place i0 and i1 in an int2 : left/right</param>
        /// <param name="config"></param>
        /// <param name="i0Pos"></param>
        /// <param name="i1Pos"></param>
        /// <param name="i0"></param>
        /// <param name="i1"></param>
        private void FindLeftRight(out int2 lr, out int config, in float2 i0Pos, in float2 i1Pos, in int i0, in int i1)
        {
            float2 offset = i1Pos - i0Pos;
            int2 i0I1 = int2(i0, i1);
            int2 i1I0 = int2(i1, i0);

            if (offset.x == 0)
            {
                config = 0; //CAREFUL WITH THIS ONE!
                lr = select(i0I1, i1I0, offset.y > 0);
            }
            else if (offset.y == 0)
            {
                config = 1;
                lr = select(i1I0, i0I1, offset.x > 0);
            }
            else
            {
                config = 2;
                lr = select(i1I0, i0I1, offset.x > 0);
            }
        }

        private void FindDoubleCellGridRange(out int2 yRange, out int2 xRange, int config, int i0, int i1, int mapNumCell)
        {
            int2 lr = int2(i0, i1);

            //Left Part
            int ly = (int)floor(lr.x / (float)mapNumCell);
            int lx = lr.x - mul(ly, mapNumCell);

            if(config != 0)
            {
                int2 YZeroOne() => select(int2(-1, 2), int2(0, 2), ly == 0);
                yRange = select(int2(-2, 2), YZeroOne(), ly == 0 || ly == 1);

                int2 XZeroOne() => select(int2(-1, 0), int2(0, 0), lx == 0);
                xRange = select(int2(-2, 0), XZeroOne(), lx == 0 || lx == 1);
            }
            else
            {
                int2 YZeroOne() => select(int2(-1, 0), int2(0, 0), ly == 0);
                yRange = select(int2(-2, 0), YZeroOne(), ly == 0 || ly == 1);

                int2 XZeroOne() => select(int2(-1, 2), int2(0, 2), lx == 0);
                xRange = select(int2(-2, 2), XZeroOne(), lx == 0 || lx == 1);
            }

            /*
            if (ly == 0) yRange = int2(0, 2);
            else if(ly == 1) yRange = int2(-1, 2);
            else yRange = int2(-2, 2);
            */
        }
    }
}
