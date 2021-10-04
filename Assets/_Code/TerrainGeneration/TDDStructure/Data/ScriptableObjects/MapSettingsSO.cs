using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Unity.Mathematics.math;
//using static KaizerWaldCode.Utils.KWmath;

public class MapSettingsSO : ScriptableObject
{
    private MapSettings mapSettings;
    public string SaveFile { get; private set; }
    public int ChunkSize { get; private set; }
    public int NumChunk { get; private set; }
    public int PointPerMeter { get; private set; }

    public int MapSize { get; private set; }
    public int ChunkPointPerAxis { get; private set; }
    public int MapPointPerAxis { get; private set; }
    public float PointSpacing { get; private set; }
    
    void RefreshPropretiesValue()
    {
        MapSize = ChunkSize * NumChunk;
        PointSpacing = 1f / (PointPerMeter - 1f);
        ChunkPointPerAxis = (ChunkSize * PointPerMeter) - (ChunkSize - 1);
        MapPointPerAxis = (NumChunk * ChunkSize) * PointPerMeter - (NumChunk * ChunkSize - 1);
    }
    
}
