using Unity.Mathematics;

namespace KaizerWaldCode.KwDelaunay
{
    public struct VoronoiCell : IVoronoiCell
    {
        public float3[] Points { get; set; }
        public int Index { get; set; }
        public VoronoiCell(int triangleIndex, float3[] points)
        {
            Points = points;
            Index = triangleIndex;
        }
    }
}
