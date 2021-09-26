using System.Collections.Generic;
using Unity.Mathematics;

namespace KaizerWaldCode.KwDelaunay
{
    public interface ITriangle
    {
        IEnumerable<float3> Points { get; }
        int Index { get; }
    }
}
