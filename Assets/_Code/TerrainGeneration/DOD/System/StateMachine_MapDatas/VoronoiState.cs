using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.TerrainGeneration.KwSystem;
using KaizerWaldCode.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static Unity.Mathematics.math;
using int2 = Unity.Mathematics.int2;

using dir = KaizerWaldCode.TerrainGeneration.Directories_MapGeneration;

namespace KaizerWaldCode.TerrainGeneration
{
    public class VoronoiState : IState
    {
        readonly SettingsData mapSettings;
        readonly int mapTotalPoints;

        public VoronoiState(in SettingsData mapSettings)
        {
            this.mapSettings = mapSettings;
            mapTotalPoints = sq(in mapSettings.MapPointPerAxis);
        }

        public void DoState()
        {
            VoronoiMap();
        }

        private void VoronoiMap(in JobHandle dependency = new JobHandle())
        {
            using NativeArray<float3> verticesPos = AllocNtvAry<float3>(mapTotalPoints);
            using NativeArray<int> verticesCellId = AllocNtvAry<int>(mapTotalPoints);
            using NativeArray<float3> samplesPos = AllocNtvAry<float3>(sq(mapSettings.NumCellMap));
            //Get Values From Json
            verticesPos.CopyFrom(JsonHelper.FromJson<float3>(dir.GetFile_MapAt(MapFiles.VerticesPos)));
            verticesCellId.CopyFrom(JsonHelper.FromJson<int>(dir.GetFile_MapAt(MapFiles.VerticesCellIndex)));
            samplesPos.CopyFrom(JsonHelper.FromJson<float3>(dir.GetFile_MapAt(MapFiles.PoissonDiscPos)));

            using NativeArray<int> voronoies = AllocNtvAry<int>(in mapTotalPoints);

            VoronoiCellGridJob job = new VoronoiCellGridJob()
            {
                NumCellJob = mapSettings.NumCellMap,
                JVerticesPos = verticesPos,
                JSamplesPos = samplesPos,
                JVerticesCellIndex = verticesCellId,
                JVoronoiVertices = voronoies,
            };
            JobHandle jobHandle = job.ScheduleParallel(mapTotalPoints, JobsUtility.JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            JsonHelper.ToJson(voronoies, dir.GetFile_MapAt(MapFiles.Voronoi));
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct VoronoiCellGridJob : IJobFor
        {
            [ReadOnly] public int NumCellJob;

            [ReadOnly] public NativeArray<float3> JVerticesPos;
            [ReadOnly] public NativeArray<int> JVerticesCellIndex;
            [ReadOnly] public NativeArray<float3> JSamplesPos;

            [NativeDisableParallelForRestriction] [WriteOnly]
            public NativeArray<int> JVoronoiVertices;

            public void Execute(int index)
            {
                int2 xRange;
                int2 yRange;
                int numCell;
                CellGridRanges(JVerticesCellIndex[index], out xRange, out yRange, out numCell);
                
                NativeArray<int> cellsIndex = new NativeArray<int>(numCell, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                int cellCount = 0;
                for (int y = yRange.x; y <= yRange.y; y++)
                {
                    for (int x = xRange.x; x <= xRange.y; x++)
                    {
                        int indexCellOffset = JVerticesCellIndex[index] + mad(y, NumCellJob, x);
                        cellsIndex[cellCount] = indexCellOffset;
                        cellCount++;
                    }
                }
                
                NativeArray<float> distances = new NativeArray<float>(numCell, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                for (int i = 0; i < numCell; i++)
                {
                    distances[i] = distancesq(JVerticesPos[index], JSamplesPos[cellsIndex[i]]);
                }

                JVoronoiVertices[index] = IndexMin(in distances, in cellsIndex);
            }

            /// <summary>
            /// Get both X/Y grid Range (neighbores around the cell)
            /// Get numCell to check (may be less if the cell checked is on a corner or on an edge of the grid)
            /// </summary>
            /// <param name="cell">index of the current cell checked</param>
            /// <param name="xRange"></param>
            /// <param name="yRange"></param>
            /// <param name="numCell"></param>
            void CellGridRanges(int cell, out int2 xRange, out int2 yRange, out int numCell)
            {
                int y = (int) floor((float) cell / NumCellJob);
                int x = cell - mul(y, NumCellJob);

                bool corner = (x == 0 && y == 0) || (x == 0 && y == NumCellJob - 1) ||
                              (x == NumCellJob - 1 && y == 0) || (x == NumCellJob - 1 && y == NumCellJob - 1);
                bool yOnEdge = y == 0 || y == NumCellJob - 1;
                bool xOnEdge = x == 0 || x == NumCellJob - 1;

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
            int IndexMin(in NativeArray<float> dis, in NativeArray<int> cellIndex)
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
        }
    }
}
