using Unity.Mathematics;

namespace KaizerWaldCode.KwDelaunay
{
    public interface IEdge
    {
        float3 P { get; }
        float3 Q { get; }
        int Index { get; }
    }
}
