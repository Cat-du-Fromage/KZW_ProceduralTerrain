using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapSettings
{
    public string SaveName { get; }
    int chunkSize;
    int numChunk;
    int pointPerMeter;

    public MapSettings(int chunkSize, int numChunk, int pointPerMeter, string saveName = "DefaultSaveName")
    {
        SaveName = saveName == String.Empty ? "DefaultSaveName" : saveName;
        this.chunkSize = chunkSize;
        this.numChunk = numChunk;
        this.pointPerMeter = pointPerMeter;
    }
}
