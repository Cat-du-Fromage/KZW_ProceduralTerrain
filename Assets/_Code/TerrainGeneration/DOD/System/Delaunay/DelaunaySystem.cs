using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.TerrainGeneration.KwSystem;
using KaizerWaldCode.Utils;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.Utils.KWmath;

using float2 = Unity.Mathematics.float2;
using float3 = Unity.Mathematics.float3;

namespace KaizerWaldCode.KwDelaunay
{
    
    public partial class DelaunaySystem
    {
        private readonly int[] EDGE_STACK = new int[512];
        
        private NativeArray<float3> coords;
        private NativeArray<int> coordsId;

        private NativeArray<int> triangles;
        private NativeArray<int> Halfedges;

        private int              hashSize;
        private NativeArray<int> hullPrev; // not keeped
        private NativeArray<int> hullNext; // not keeped
        private NativeArray<int> hullTri; // not keeped
        private NativeArray<int> hullHash;

        private float2 center;

        private int trianglesLen;
        private int hullStart;
        private int hullSize;
        private NativeArray<int> hull;

        public DelaunaySystem(in string savePath, in MapSettingsData set)
        {
            MapDirectories dir = new MapDirectories();
            dir.SelectedSave = savePath;
            //=============================================================
            //Get Poisson Disc Pos From Json
            //=============================================================
            coords = AllocNtvAry<float3>(sq(set.NumCellMap));
            coords.CopyFrom(JsonHelper.FromJson<float3>(dir.GetFullMapFileAt((int)MapFiles.PoissonDiscPos)));
            
            //=============================================================
            //DELAUNAY IMPOSSIBLE
            //=============================================================
            if (coords.Length < 3) return;
            int n = coords.Length;
            int maxTriangles = mad(2, n, -5);

            //=================================================================================
            //Get Ids from save files
            //maybe not needed since its only needed to get nearest point of circumcenter later
            //=================================================================================
            coordsId = AllocNtvAry<int>(n);
            coordsId.CopyFrom(JsonHelper.FromJson<int>(dir.GetFullMapFileAt((int)MapFiles.PoissonDiscId)));
            
            int i0 = 0;
            int i1 = 0;
            int i2 = 0;

            float3 i0Pos = float3.zero;
            float3 i1Pos = float3.zero;
            float3 i2Pos = float3.zero;
            
            center = float2(set.MapSize * 0.5f);// CAREFULL Try with float2.zero if there is a bug
            int centerCellId = KwGrid.Get2DCellID(center, set.NumCellMap, set.CellSize);
            
            (i0, i0Pos) = InitI0(coords, centerCellId);

            (i1, i1Pos) = InitI1(i0, i0Pos, coords, set.NumCellMap);

            (i2, i2Pos) = InitI2(i0, i1, set.NumCellMap, coords);

            if (IsLeft(i0Pos.xz, i1Pos.xz, i2Pos.xz))
            {
                int i = i1;
                float x = i1Pos.x;
                float z = i1Pos.z;
                i1 = i2;
                i1Pos.x = i2Pos.x;
                i1Pos.z = i2Pos.z;
                i2 = i;
                i2Pos.x = x;
                i2Pos.z = z;
            }
            center = GetCircumcenter(i0Pos.xz, i1Pos.xz, i2Pos.xz);
            //Make an array sorting PoissonDiscArrayID by distances from center/circumcenter
            NativeArray<float> sortedDst = AllocNtvAry<float>(n);
            
            JobHandle sortJob = GetSortedDstCoords(center, coords, ref sortedDst);
            sortJob.Complete();
            Quicksort(ref coordsId, sortedDst, 0, n - 1);
            //init HULLS NativeArray
            InitHulls(n, i0, i1, i2, i0Pos, i1Pos, i2Pos);
            trianglesLen = 0;
            hullStart = i0;
            hullSize = 3;
            
            triangles = AllocNtvAry<int>(maxTriangles * 3);
            Halfedges = AllocNtvAry<int>(maxTriangles * 3);

            AddTriangle(i0, i1, i2, -1, -1, -1);
            //go through all the other points and add triangles
            TrianglesLoop(i0, i1, i2);
            
            JsonHelper.ToJson(triangles, dir.GetDelaunayFileAt(DelaunayFiles.triangles));
            JsonHelper.ToJson(Halfedges, dir.GetDelaunayFileAt(DelaunayFiles.halfedges));
            JsonHelper.ToJson(hull, dir.GetDelaunayFileAt(DelaunayFiles.hull));
            
            sortedDst.Dispose();
            DisposeAll();
        }

        private void TrianglesLoop(in int i0, in int i1, in int i2)
        {
            float xp = 0;
            float yp = 0;
            for (int k = 0; k < coordsId.Length; k++)
            {
                int i = coordsId[k];
                float x = coords[i].x;
                float y = coords[i].z;
                float2 coordsI = float2(x, y);

                // skip near-duplicate points
                if (k > 0 && abs(x - xp) <= EPSILON && abs(y - yp) <= EPSILON) continue;
                xp = x;
                yp = y;

                // skip seed triangle points
                if (i == i0 || i == i1 || i == i2) continue;

                // find a visible edge on the convex hull using edge hash
                int start = 0;
                for (int j = 0; j < hashSize; j++)
                {
                    int key = HashKey(coordsI);
                    start = hullHash[(key + j) % hashSize];
                    if (start != -1 && start != hullNext[start]) break;
                }


                start = hullPrev[start];
                int e = start;
                int q = hullNext[e];

                while (!IsLeft(coordsI, coords[e].xz, coords[q].xz))
                {
                    e = q;
                    if (e == start)
                    {
                        e = int.MaxValue;
                        break;
                    }

                    q = hullNext[e];
                }

                if (e == int.MaxValue) continue; // likely a near-duplicate point; skip it

                // add the first triangle from the point
                int t = AddTriangle(e, i, hullNext[e], -1, -1, hullTri[e]);

                // recursively flip triangles from the point until they satisfy the Delaunay condition
                hullTri[i] = Legalize(t + 2);
                hullTri[e] = t; // keep track of boundary triangles on the hull
                hullSize++;

                // walk forward through the hull, adding more triangles and flipping recursively
                int next = hullNext[e];
                q = hullNext[next];

                while (IsLeft(float2(x, y), coords[next].xz, coords[q].xz))
                {
                    t = AddTriangle(next, i, q, hullTri[i], -1, hullTri[next]);
                    hullTri[i] = Legalize(t + 2);
                    hullNext[next] = next; // mark as removed
                    hullSize--;
                    next = q;

                    q = hullNext[next];
                }

                // walk backward from the other side, adding more triangles and flipping
                if (e == start)
                {
                    q = hullPrev[e];

                    while (IsLeft(coordsI, coords[q].xz, coords[e].xz))
                    {
                        t = AddTriangle(q, i, e, -1, hullTri[e], hullTri[q]);
                        Legalize(t + 2); // is t changed?
                        hullTri[q] = t;
                        hullNext[e] = e; // mark as removed
                        hullSize--;
                        e = q;

                        q = hullPrev[e];
                    }
                }

                // update the hull indices
                hullStart = hullPrev[i] = e;
                hullNext[e] = hullPrev[next] = i;
                hullNext[i] = next;

                // save the two new edges in the hash table
                hullHash[HashKey(coordsI)] = i;
                hullHash[HashKey(coords[e].xz)] = e;
            }

            hull = AllocNtvAry<int>(hullSize);
            int s = hullStart;
            for (int i = 0; i < hullSize; i++)
            {
                hull[i] = s;
                s = hullNext[s];
            }
            
            //hullPrev = hullNext = hullTri = null; // get rid of temporary arrays
            //// trim typed triangle mesh arrays
            int[] tempTriangles = new int[triangles.Length];
            int[] tempHalfEdges = new int[Halfedges.Length];
            tempTriangles = triangles.Take(trianglesLen).ToArray();
            tempHalfEdges = Halfedges.Take(trianglesLen).ToArray();
            
            DisposeHulls();
            triangles.Dispose();
            Halfedges.Dispose();
            
            triangles = ArrayToNativeArray(tempTriangles);
            Halfedges = ArrayToNativeArray(tempHalfEdges);
        }
        
        private int Legalize(int a)
        {
            int i = 0;
            int ar;

            // recursion eliminated with a fixed-size stack
            while (true)
            {
                int b = Halfedges[a];

                /* if the pair of triangles doesn't satisfy the Delaunay condition
                 * (p1 is inside the circumcircle of [p0, pl, pr]), flip them,
                 * then do the same check/flip recursively for the new pair of triangles
                 *
                 *           pl                    pl
                 *          /||\                  /  \
                 *       al/ || \bl            al/    \a
                 *        /  ||  \              /      \
                 *       /  a||b  \    flip    /___ar___\
                 *     p0\   ||   /p1   =>   p0\---bl---/p1
                 *        \  ||  /              \      /
                 *       ar\ || /br             b\    /br
                 *          \||/                  \  /
                 *           pr                    pr
                 */
                int a0 = a - a % 3;
                ar = a0 + (a + 2) % 3;

                if (b == -1)
                { // convex hull edge
                    if (i == 0) break;
                    a = EDGE_STACK[--i];
                    continue;
                }

                int b0 = b - b % 3;
                int al = a0 + (a + 1) % 3;
                int bl = b0 + (b + 2) % 3;

                int p0 = triangles[ar];
                int pr = triangles[a];
                int pl = triangles[al];
                int p1 = triangles[bl];

                bool illegal = InCircle(coords[p0].xz, coords[pr].xz, coords[pl].xz, coords[p1].xz);
                if (illegal)
                {
                    triangles[a] = p1;
                    triangles[b] = p0;

                    int hbl = Halfedges[bl];

                    // edge swapped on the other side of the hull (rare); fix the halfedge reference
                    if (hbl == -1)
                    {
                        int e = hullStart;
                        do
                        {
                            if (hullTri[e] == bl)
                            {
                                hullTri[e] = a;
                                break;
                            }
                            e = hullPrev[e];
                        } while (e != hullStart);
                    }
                    Link(a, hbl);
                    Link(b, Halfedges[ar]);
                    Link(ar, bl);
                    int br = b0 + (b + 1) % 3;

                    // don't worry about hitting the cap: it can only happen on extremely degenerate input
                    if (i < EDGE_STACK.Length)
                    {
                        EDGE_STACK[i++] = br;
                    }
                }
                else
                {
                    if (i == 0) break;
                    a = EDGE_STACK[--i];
                }
            }
            return ar;
        }

        /// <summary>
        /// Initialize Native Array and assign the I0/I1/I2 previously calculated
        /// </summary>
        /// <param name="size">num of poissonDisc</param>
        /// <param name="i0"></param>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <param name="i0Pos"></param>
        /// <param name="i1Pos"></param>
        /// <param name="i2Pos"></param>
        private void InitHulls(in int size, in int i0, in int i1, in int i2, in float3 i0Pos, in float3 i1Pos, in float3 i2Pos)
        {
            hashSize = (int)ceil(sqrt(size));
            hullPrev = AllocNtvAry<int>(size);
            hullNext = AllocNtvAry<int>(size);
            hullTri  = AllocNtvAry<int>(size);
            hullHash = AllocNtvAry<int>(hashSize);
            
            hullNext[i0] = hullPrev[i2] = i1;
            hullNext[i1] = hullPrev[i0] = i2;
            hullNext[i2] = hullPrev[i1] = i0;

            hullTri[i0] = 0;
            hullTri[i1] = 1;
            hullTri[i2] = 2;

            hullHash[HashKey(i0Pos.xz)] = i0;
            hullHash[HashKey(i1Pos.xz)] = i1;
            hullHash[HashKey(i2Pos.xz)] = i2;
        }

        private void DisposeHulls()
        {
            if (hullPrev.IsCreated) hullPrev.Dispose();
            if (hullNext.IsCreated) hullNext.Dispose();
            if (hullTri.IsCreated) hullTri.Dispose();
        }
        private void DisposeAll()
        {
            if (coords.IsCreated) coords.Dispose();
            if (coordsId.IsCreated) coordsId.Dispose();
            if (hullPrev.IsCreated) hullPrev.Dispose();
            if (hullNext.IsCreated) hullNext.Dispose();
            if (hullTri.IsCreated) hullTri.Dispose();
            if (hullHash.IsCreated) hullHash.Dispose();
            if (triangles.IsCreated) triangles.Dispose();
            if (Halfedges.IsCreated) Halfedges.Dispose();
            if (hull.IsCreated) hull.Dispose();
        }
    }
}
