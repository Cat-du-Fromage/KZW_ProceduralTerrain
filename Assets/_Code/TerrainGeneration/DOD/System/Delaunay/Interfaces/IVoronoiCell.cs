using System.Collections.Generic;
using Unity.Mathematics;

namespace KaizerWaldCode.TerrainGeneration.KwDelaunay
{
    public interface IVoronoiCell
    {
        float3[] Points { get; }
        int Index { get; }
    }
}
