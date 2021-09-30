using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KaizerWaldCode.Utils;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using KaizerWaldCode.TerrainGeneration.KwSystem;

using static KaizerWaldCode.Utils.KWmath;
using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;

namespace KaizerWaldCode.KwDelaunay
{
    public class DelaunayMethods
    {
        private readonly MapDirectories dir;
        public int[] Triangles { get; private set; }
        public int[] Halfedges { get; private set; }
        public float3[] Points { get; private set; }
        
        private readonly int[] hull;
        
        public DelaunayMethods(in string path)
        {
            dir = new MapDirectories(path);
            Triangles = JsonHelper.FromJson<int>(dir.GetDelaunayFileAt(DelaunayFiles.triangles));
            Halfedges = JsonHelper.FromJson<int>(dir.GetDelaunayFileAt(DelaunayFiles.halfedges));
            hull = JsonHelper.FromJson<int>(dir.GetDelaunayFileAt(DelaunayFiles.hull));
            Points = JsonHelper.FromJson<float3>(dir.GetFullMapFileAt((int)MapFiles.PoissonDiscPos));
        }
        
        #region GetMethods
        public IEnumerable<ITriangle> GetTriangles()
        {
            for (var t = 0; t < Triangles.Length / 3; t++)
            {
                yield return new Triangle(t, GetTrianglePoints(t));
            }
        }
        public IEnumerable<IEdge> GetEdges()
        {
            for (int e = 0; e < Triangles.Length; e++)
            {
                if (e > Halfedges[e])
                {
                    float3 p = Points[Triangles[e]];
                    float3 q = Points[Triangles[NextHalfedge(e)]];
                    yield return new Edge(e, p, q);
                }
            }
        }
        public IEnumerable<IEdge> GetVoronoiEdges(Func<int, float3> triangleVerticeSelector = null)
        {
            if (triangleVerticeSelector is null) triangleVerticeSelector = x => GetCentroid(x);
            for (int e = 0; e < Triangles.Length; e++)
            {
                if (e < Halfedges[e])
                {
                    float3 p = triangleVerticeSelector(TriangleOfEdge(e));
                    float3 q = triangleVerticeSelector(TriangleOfEdge(Halfedges[e]));
                    yield return new Edge(e, p, q);
                }
            }
        }

        public IEnumerable<IEdge> GetVoronoiEdgesBasedOnCircumCenter() => GetVoronoiEdges(GetTriangleCircumcenter);
        public IEnumerable<IEdge> GetVoronoiEdgesBasedOnCentroids() => GetVoronoiEdges(GetCentroid);

        public IEnumerable<IVoronoiCell> GetVoronoiCells(Func<int, float3> triangleVerticeSelector = null)
        {
            if (triangleVerticeSelector is null) triangleVerticeSelector = x => GetCentroid(x);

            HashSet<int> seen = new HashSet<int>();
            List<float3> vertices = new List<float3>(10);    // Keep it outside the loop, reuse capacity, less resizes.

            for (int triangleId = 0; triangleId < Triangles.Length; triangleId++)
            {
                int id = Triangles[NextHalfedge(triangleId)];
                // True if element was added, If resize the set? O(n) : O(1)
                if (seen.Add(id))
                {
                    foreach (int edge in EdgesAroundPoint(triangleId))
                    {
                        // triangleVerticeSelector cant be null, no need to check before invoke (?.).
                        vertices.Add(triangleVerticeSelector.Invoke(TriangleOfEdge(edge)));
                    }
                    yield return new VoronoiCell(id, vertices.ToArray());
                    vertices.Clear();   // Clear elements, keep capacity
                }
            }
        }

        public IEnumerable<IVoronoiCell> GetVoronoiCellsBasedOnCircumcenters() => GetVoronoiCells(GetTriangleCircumcenter);
        public IEnumerable<IVoronoiCell> GetVoronoiCellsBasedOnCentroids() => GetVoronoiCells(GetCentroid);

        public IEnumerable<IEdge> GetHullEdges() => CreateHull(GetHullPoints());

        public float3[] GetHullPoints() => Array.ConvertAll<int, float3>(hull, (x) => Points[x]);

        public float3[] GetTrianglePoints(int t)
        {
            List<float3> points = new List<float3>();
            foreach (int p in PointsOfTriangle(t))
            {
                points.Add(Points[p]);
            }
            return points.ToArray();
        }

        public float3[] GetRellaxedPoints()
        {
            List<float3> points = new List<float3>();
            foreach (IVoronoiCell cell in GetVoronoiCellsBasedOnCircumcenters())
            {
                points.Add(GetCentroid(cell.Points));
            }
            return points.ToArray();
        }

        public IEnumerable<IEdge> GetEdgesOfTriangle(int t) => CreateHull(EdgesOfTriangle(t).Select(p => Points[p]));
        public static IEnumerable<IEdge> CreateHull(IEnumerable<float3> points) => points.Zip(points.Skip(1).Append(points.FirstOrDefault()), (a, b) => new Edge(0, a, b)).OfType<IEdge>();
        public float3 GetTriangleCircumcenter(int t)
        {
            float3[] vertices = GetTrianglePoints(t);
            float2 c = GetCircumcenter(vertices[0].xz, vertices[1].xz, vertices[2].xz);
            return float3(c.x,0,c.y);
        }
        public float3 GetCentroid(int t)
        {
            float3[] vertices = GetTrianglePoints(t);
            return GetCentroid(vertices);
        }
        //public static IPoint GetCircumcenter(IPoint a, IPoint b, IPoint c) => KWmath.GetCircumcenter(float2(a.X,a.Y), float2(b.X, b.Y), float2(c.X, c.Y));

        public static float3 GetCentroid(float3[] points)
        {
            float accumulatedArea = 0.0f;
            float centerX = 0.0f;
            float centerY = 0.0f;

            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
            {
                float temp = points[i].x * points[j].z - points[j].x * points[i].z;
                accumulatedArea += temp;
                centerX += (points[i].x + points[j].x) * temp;
                centerY += (points[i].z + points[j].z) * temp;
            }

            if (abs(accumulatedArea) < 1E-7f)
                return new float3();

            accumulatedArea *= 3f;
            return new float3(centerX / accumulatedArea,0 ,centerY / accumulatedArea);
        }

        #endregion GetMethods

        #region ForEachMethods
        public void ForEachTriangle(Action<ITriangle> callback)
        {
            foreach (ITriangle triangle in GetTriangles())
            {
                callback?.Invoke(triangle);
            }
        }
        public void ForEachTriangleEdge(Action<IEdge> callback)
        {
            foreach (IEdge edge in GetEdges())
            {
                callback?.Invoke(edge);
            }
        }
        public void ForEachVoronoiEdge(Action<IEdge> callback)
        {
            foreach (IEdge edge in GetVoronoiEdges())
            {
                callback?.Invoke(edge);
            }
        }
        public void ForEachVoronoiCellBasedOnCentroids(Action<IVoronoiCell> callback)
        {
            foreach (IVoronoiCell cell in GetVoronoiCellsBasedOnCentroids())
            {
                callback?.Invoke(cell);
            }
        }

        public void ForEachVoronoiCellBasedOnCircumcenters(Action<IVoronoiCell> callback)
        {
            foreach (IVoronoiCell cell in GetVoronoiCellsBasedOnCircumcenters())
            {
                callback?.Invoke(cell);
            }
        }

        public void ForEachVoronoiCell(Action<IVoronoiCell> callback, Func<int, float3> triangleVertexSelector = null)
        {
            foreach (IVoronoiCell cell in GetVoronoiCells(triangleVertexSelector))
            {
                callback?.Invoke(cell);
            }
        }

        #endregion ForEachMethods

        #region Methods based on index
        public IEnumerable<int> EdgesAroundPoint(int start)
        {
            int incoming = start;
            do
            {
                yield return incoming;
                int outgoing = NextHalfedge(incoming);
                incoming = Halfedges[outgoing];
            } while (incoming != -1 && incoming != start);
        }
        public IEnumerable<int> PointsOfTriangle(int t)
        {
            foreach (int edge in EdgesOfTriangle(t))
            {
                yield return Triangles[edge];
            }
        }
        public IEnumerable<int> TrianglesAdjacentToTriangle(int t)
        {
            List<int> adjacentTriangles = new List<int>();
            int[] triangleEdges = EdgesOfTriangle(t);
            foreach (int e in triangleEdges)
            {
                int opposite = Halfedges[e];
                if (opposite >= 0)
                {
                    adjacentTriangles.Add(TriangleOfEdge(opposite));
                }
            }
            return adjacentTriangles;
        }

        public static int NextHalfedge(int e) => (e % 3 == 2) ? e - 2 : e + 1;
        public static int PreviousHalfedge(int e) => (e % 3 == 0) ? e + 2 : e - 1;
        public static int[] EdgesOfTriangle(int t) => new int[] { 3 * t, 3 * t + 1, 3 * t + 2 };
        public static int TriangleOfEdge(int e) { return e / 3; }
        #endregion Methods based on index
    
    }
}
