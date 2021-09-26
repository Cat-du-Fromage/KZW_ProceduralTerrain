using Unity.Mathematics;

namespace KaizerWaldCode.KwDelaunay
{
    public struct Edge : IEdge
    {
        public float3 P { get; set; }
        public float3 Q { get; set; }
        public int Index { get; set; }

        public Edge(int e, float3 p, float3 q)
        {
            Index = e;
            P = p;
            Q = q;
        }
    }
}
