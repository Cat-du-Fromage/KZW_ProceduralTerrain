using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace KaizerWaldCode.Utils
{
    public static class KwGrid
    {
        /// <summary>
        /// Get position X and Y of a 1D Grid from an index
        /// </summary>
        /// <param name="i">index</param>
        /// <param name="w">width of the grid</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GetXY2(in int i, in int w)
        {
            int y = (int)floor((float)i/w);
            int x = i - (y * w);
            return int2(x, y);
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int,int) GetXY(in int i, in int w)
        {
            int y = (int)floor((float)i / w);
            int x = i - (y * w);
            return (x, y);
        }
        
        //=====================================
        //START : MIN INDEX
        //=====================================
        /// <summary>
        /// Find the index of the minimum value of an array
        /// </summary>
        /// <param name="dis">array containing float distance value from point to his neighbors</param>
        /// <param name="cellIndex">HashGrid indices</param>
        /// <returns>index of the closest point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexMin(in NativeArray<float> dis, in NativeArray<int> cellIndex)
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexMin(in NativeArray<int> dis, in NativeArray<int> cellIndex)
        {
            int val = int.MaxValue;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexMin(in NativeArray<double> dis, in NativeArray<int> cellIndex)
        {
            double val = double.MaxValue;
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
        //=====================================
        // END : MIN INDEX
        //=====================================
        
        //=====================================
        //HASHGRID : Cell Grid
        //You need to precompute the hashGrid to use the function
        //may need either : NativeArray<Position> PointInsideHashGrid OR NativeArray<ID> CellId containing the point
        //=====================================
        
        /// <summary>
        /// Find the index of the cells a point belongs to
        /// </summary>
        /// <param name="pointPos">point from where we want to find the cell</param>
        /// <param name="numCellMap">number of cells per axis (fullmap : mapSize * numChunk / radius)</param>
        /// <param name="cellRadius">radius on map settings</param>
        /// <returns>index of the cell</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Get2DCellID(in float2 pointPos, in int numCellMap, in int cellSize)
        {
            int2 cellGrid = int2(numCellMap);
            for (int i = 0; i < numCellMap; i++)
            {
                if (cellGrid.y == numCellMap) cellGrid.y = select(numCellMap, i, pointPos.y <= mad(i, cellSize, cellSize));
                if (cellGrid.x == numCellMap) cellGrid.x = select(numCellMap, i, pointPos.x <= mad(i, cellSize, cellSize));
                if (cellGrid.x != numCellMap && cellGrid.y != numCellMap) break;
            }
            return mad(cellGrid.y, numCellMap, cellGrid.x);
        }
        
        /// <summary>
        /// Find the index of the cells a point belongs to
        /// </summary>
        /// <param name="pointPos">point from where we want to find the cell</param>
        /// <param name="numCellMap">number of cells per axis (fullmap : mapSize * numChunk / radius)</param>
        /// <param name="cellRadius">radius on map settings</param>
        /// <returns>index of the cell</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Get2DCellID(in float3 pointPos, in int numCellMap, in int cellSize)
        {
            int2 cellGrid = int2(numCellMap);
            for (int i = 0; i < numCellMap; i++)
            {
                if (cellGrid.y == numCellMap) cellGrid.y = select(numCellMap, i, pointPos.z <= mad(i, cellSize, cellSize));
                if (cellGrid.x == numCellMap) cellGrid.x = select(numCellMap, i, pointPos.x <= mad(i, cellSize, cellSize));
                if (cellGrid.x != numCellMap && cellGrid.y != numCellMap) break;
            }
            return mad(cellGrid.y, numCellMap, cellGrid.x);
        }
        
    }
}
