using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.Mathematics;
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
            Stopwatch sw = new Stopwatch();
            sw.Start();
            dir.SelectedSave = folderName;

            //"Directory.CreateDirectory" create all missing directory in the path(does not create a duplicate if already exist)
            if (!Directory.Exists(dir.MapDatasPath)) { Directory.CreateDirectory(dir.MapDatasPath); }

            if (!Directory.Exists(dir.FullMapDatasPath)) 
            { 
                Directory.CreateDirectory(dir.FullMapDatasPath);
                CreateFullMapFiles(false);
            }
            else
            {
                CreateFullMapFiles(true);
            }

            if (!Directory.Exists(dir.ChunksPath)) { Directory.CreateDirectory(dir.ChunksPath); }
            CreateTrianglesFile();

            //individual Chunks Folder
            //string chunkPath;
            for (int i = 0; i < sq(mapSettings.NumChunk); i++)
            {
                int ChunkPosY = (int)floor(i / (float)mapSettings.NumChunk);
                int ChunkPosX = i - (ChunkPosY * mapSettings.NumChunk);

                if (!Directory.Exists(dir.GetChunk(ChunkPosX, ChunkPosY))) 
                {
                    Directory.CreateDirectory(dir.GetChunk(ChunkPosX, ChunkPosY));
                    CreateChunksFiles(ChunkPosX, ChunkPosY, false);
                }
                else
                {
                    CreateChunksFiles(ChunkPosX, ChunkPosY, true);
                }
            }
            sw.Stop();
            UnityEngine.Debug.Log($"GetOrCreateDirectories = {sw.Elapsed}");
        }
    }

    [Serializable]
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

        #region FILES

        public readonly string GetFullMapFileAt(in int file)
        {
            return $"{FullMapDatasPath}{FullMapFilesPath[file]}";
        }

        public readonly string GetChunksTriangleFile()
        {
            return $"{ChunksPath}{ChunksTrianglesFile}";
        }
        /// <summary>
        /// Return the path to a given chunk (depending on x,y value entered)
        /// </summary>
        /// <param name="x">X Position of the chunk</param>
        /// <param name="y">Y position of the chunk</param>
        /// <param name="file"></param>
        /// <returns></returns>
        public readonly string GetChunkFileAt(in int x, in int y, in int file)
        {
            if (file == 3)
                return GetChunksTriangleFile();
            else
                return $"{GetChunk(x, y)}{ChunkFilesPath[file]}";
        }

        public readonly string GetChunkFileAt(in int2 Pos, in int file)
        {
            if (file == 3)
                return GetChunksTriangleFile();
            else
                return $"{GetChunk(Pos.x, Pos.y)}{ChunkFilesPath[file]}";
        }


        /// <summary>
        /// Array containing all files path for the map as a whole
        /// </summary>
        public readonly string[] FullMapFilesPath
        {
            get
            {
                return new string[]
                {
                    @"\VerticesPosition.txt",
                    @"\VerticesCellIndex.txt",
                    @"\PoissonDiscPosition.txt",
                    @"\PoissonDiscCellIndex.txt",
                    @"\Voronoi.txt",
                    @"\IslandShape.txt",
                    @"\Noise.txt",
                    @"\FallOff.txt",
                };
            }
        }

        //triangles are the same for each chunk (since it only define the draw order of the mesh)
        private const string ChunksTrianglesFile = @"\Triangles.txt";

        /// <summary>
        /// Array containing all files path for each chunk
        /// </summary>
        public readonly string[] ChunkFilesPath
        {
            get
            {
                return new string[]
                {
                    @"\VerticesPosition.txt",
                    @"\VerticesCellIndex.txt",
                    @"\Uvs.txt",
                    @"\PoissonDiscPosition.txt",
                    @"\PoissonDiscCellIndex.txt",
                };
            }
        }
        #endregion FILES
    }

    [Flags]
    public enum ChunkFiles : int
    {
        VerticesPos = 0,
        VerticesCellIndex = 1,
        Uvs = 2,
        Triangles = 3,
    }
    [Flags]
    public enum FullMapFiles : int
    {
        VerticesPos = 0,
        VerticesCellIndex = 1,
    }
}
