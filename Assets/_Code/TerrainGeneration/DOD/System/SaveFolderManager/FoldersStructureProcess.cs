using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static KaizerWaldCode.Utils.KWmath;
using static Unity.Mathematics.math;
namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    /// <summary>
    /// Save Directory Structure
    /// Check/Create all Directory needed for the Map generation persistent data
    /// </summary>
    public partial class MapSystem : MonoBehaviour
    {
        private MapDirectories dir;

        private void GetOrCreateDirectories(in string folderName)
        {
            dir.SelectedSave = folderName;

            //"Directory.CreateDirectory" create all missing directory in the path(does not create a duplicate if already exist)
            if (!Directory.Exists(dir.MapDatasPath)) { Directory.CreateDirectory(dir.MapDatasPath); }
            if (!Directory.Exists(dir.FullMapDatasPath)) { Directory.CreateDirectory(dir.FullMapDatasPath); }
            if (!Directory.Exists(dir.ChunksPath)) { Directory.CreateDirectory(dir.ChunksPath); }

            //individual Chunks Folder
            //string chunkPath;
            for (int i = 0; i < sq(mapSettings.NumChunk); i++)
            {
                int ChunkPosY = (int)floor(i / (float)mapSettings.NumChunk);
                int ChunkPosX = i - (ChunkPosY * mapSettings.NumChunk);
                //string chunkDir = dir.GetChunk(ChunkPosX, ChunkPosY);

                //chunkPath = Path.Combine(dir.ChunksPath, chunkDir); // full path -> MapDatas -> ChunkX(PosX)Y(PosY)
                if (!Directory.Exists(dir.GetChunk(ChunkPosX, ChunkPosY))) { Directory.CreateDirectory(dir.GetChunk(ChunkPosX, ChunkPosY)); }
            }
        }
    }

    [System.Serializable]
    struct MapDirectories
    {
        public string SelectedSave;

        const string SaveFiles = "SaveFiles";
        const string MapDatas = "MapDatas";
        const string FullMapDatas = "FullMapDatas";
        const string Chunks = "Chunks";
        const string Chunk = "Chunk";

        public readonly string GetChunk(int x, int y)
        {
            return Path.Combine(ChunksPath, $"{Chunk}X{x}Y{y}");
        }

        public readonly string SelectSavePath { get {return Path.Combine(Application.persistentDataPath, SaveFiles, SelectedSave); } }
        public readonly string MapDatasPath { get { return Path.Combine(SelectSavePath, MapDatas); } }
        public readonly string FullMapDatasPath { get { return Path.Combine(MapDatasPath, FullMapDatas); } }
        public readonly string ChunksPath { get { return Path.Combine(MapDatasPath, Chunks); } }
    }
}
