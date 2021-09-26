using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using KaizerWaldCode.Utils;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.Utils.KWmath;

using int2 = Unity.Mathematics.int2;
using float2 = Unity.Mathematics.float2;
using float3 = Unity.Mathematics.float3;

namespace KaizerWaldCode.KwDelaunay
{
    public static class DelaunayI2Utils
    {
        public static NativeArray<int> GetLowGridIDS(in int mapNumCell, in int lowId, in int2 lowXY)
        {
            bool leftCorner = (lowXY.x == 0 & lowXY.y == 0) | (lowXY.x == 0 & lowXY.y == mapNumCell - 1);
            bool yOnEdge, xOnEdge;
            
            yOnEdge = lowXY.y == 0 | lowXY.y == mapNumCell - 1;
            xOnEdge = lowXY.x == 0 | lowXY.x == mapNumCell - 1;

            int2 OnEdge(int e) => e == 0 ? int2(0, 1) : int2(-1, 0); // if not 0 then it means e = mapNumCell - 1
            int2 yRange = !yOnEdge ? int2(-1, 1) : OnEdge(lowXY.y);
            int2 xRange = !xOnEdge ? int2(-1, 1) : OnEdge(lowXY.x);
            
            int numCell = (leftCorner ? 4 : (yOnEdge | xOnEdge ? 6 : 9)) - 1; // -1 for the lowId itself

            NativeArray<int> lowGridId = new NativeArray<int>(numCell,Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            int cellCount = 0;
            for (int y = yRange.x; y <= yRange.y; y++)
            {
                for (int x = xRange.x; x <= xRange.y; x++)
                {
                    int indexCellOffset = lowId + mad(y, mapNumCell, x);
                    if (indexCellOffset == lowId) continue; // lowId
                    lowGridId[cellCount] = indexCellOffset;
                    cellCount++;
                }
            }
            return lowGridId;
        }
        
        public static NativeArray<int> GetUpGridIDS(in int mapNumCell, in int upId, in int2 upXY)
        {
            bool leftCorner = (upXY.x == 0 & upXY.y == 0) | (upXY.x == 0 & upXY.y == mapNumCell - 1);
            bool yOnEdge, xOnEdge;
            
            yOnEdge = upXY.y == 0 | upXY.y == mapNumCell - 1;
            xOnEdge = upXY.x == 0 | upXY.x == mapNumCell - 1;

            int2 OnEdge(int e) => e == 0 ? int2(0, 1) : int2(-1, 0); // if not 0 then it means e = mapNumCell - 1
            int2 yRange = !yOnEdge ? int2(-1, 1) : OnEdge(upXY.y);
            int2 xRange = !xOnEdge ? int2(-1, 1) : OnEdge(upXY.x);
            
            int numCell = (leftCorner ? 4 : (yOnEdge | xOnEdge ? 6 : 9)) - 1;// -1 for the upId itself
            NativeArray<int> upGridId = new NativeArray<int>(numCell,Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            int cellCount = 0;
            for (int y = yRange.x; y <= yRange.y; y++)
            {
                for (int x = xRange.x; x <= xRange.y; x++)
                {
                    int indexCellOffset = upId + mad(y, mapNumCell, x);
                    if (indexCellOffset == upId) continue; // lowId
                    upGridId[cellCount] = indexCellOffset;
                    cellCount++;
                }
            }
            return upGridId;
        }
    }
}
