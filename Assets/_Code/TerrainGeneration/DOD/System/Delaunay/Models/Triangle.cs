using System.Collections.Generic;
using Unity.Mathematics;

namespace KaizerWaldCode.KwDelaunay
{
    public struct Triangle : ITriangle
    {
        public int Index { get; set; }

        public IEnumerable<float3> Points { get; set; }

        public Triangle(int t, IEnumerable<float3> points)
        {
            Points = points;
            Index = t;
        }
    }
}
