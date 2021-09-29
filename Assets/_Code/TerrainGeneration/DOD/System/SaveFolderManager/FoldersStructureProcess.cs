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
        private MapSettings SO;
        private MapDirectories dir;

        private void GetOrCreateDirectories(in string folderName)
        {
            //dir.SelectedSave = folderName;
            
            
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

            if (!Directory.Exists(dir.DelaunayDatasPath))
            {
                Directory.CreateDirectory(dir.DelaunayDatasPath);
                CreateDelaunayDatasFiles(false);
            }
            else
            {
                CreateDelaunayDatasFiles(true);
            }
            
            //CHunks Shared Data Directory
            if (!Directory.Exists(dir.ChunksSharedDataPath)) { Directory.CreateDirectory(dir.ChunksSharedDataPath); }
            CreateTrianglesFile();
            CreateVertexFile();
            
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
        }
    }
    
    public static class teststaticclass
    {
        private static readonly string a;

        public static string save;
        static teststaticclass()
        {
            a = save;
        }
        
    }

    [Serializable]
    public struct MapDirectories
    {
        public string SelectedSave;
        public MapDirectories(in string path)
        {
            SelectedSave = path;
            //teststaticclass.save = path;
            //teststaticclass = new teststaticclass();
        }
        
        const string SaveFiles = "SaveFiles";
        const string MapDatas = "MapDatas";
        const string FullMapDatas = "FullMapDatas";
        const string DelaunayDatas = "DelaunayDatas";
        const string Chunks = "Chunks";
        const string ChunksSharedData = "ChunksSharedData";

        public readonly string GetChunk(in int x, in int y)
        {
            return Path.Combine(ChunksPath, $"ChunkX{x}Y{y}");
        }

        public readonly string SelectSavePath { get {return Path.Combine(Application.persistentDataPath, SaveFiles, SelectedSave); } }
        public readonly string MapDatasPath { get { return Path.Combine(SelectSavePath, MapDatas); } }
        public readonly string FullMapDatasPath { get { return Path.Combine(MapDatasPath, FullMapDatas); } }
        public readonly string DelaunayDatasPath { get { return Path.Combine(FullMapDatasPath, DelaunayDatas); } }
        public readonly string ChunksPath { get { return Path.Combine(MapDatasPath, Chunks); } }
        public readonly string ChunksSharedDataPath { get { return Path.Combine(MapDatasPath, Chunks, ChunksSharedData); } }

        #region FILES
        public readonly string GetDelaunayFileAt(in DelaunayFiles file)
        {
            return $"{DelaunayDatasPath}{DelaunayFilesPath[(int)file]}";
        }
        public readonly string GetFullMapFileAt(in int file)
        {
            return $"{FullMapDatasPath}{FullMapFilesPath[file]}";
        }

        public readonly string GetChunksTriangleFile()
        {
            return $"{ChunksSharedDataPath}{ChunksTrianglesFile}";
        }
        
        public readonly string GetChunksSharedVertexFile()
        {
            return $"{ChunksSharedDataPath}{ChunksSharedVertexFile}";
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
            return $"{GetChunk(x, y)}{ChunkFilesPath[file]}";
        }

        public readonly string GetChunkFileAt(in int2 Pos, in int file)
        {
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
                    @"\VerticesPosition.json",
                    @"\VerticesCellIndex.json",
                    @"\PoissonDiscPosition.json",
                    @"\PoissonDiscCellIndex.json",
                    @"\Uvs.json",
                    @"\Voronoi.json",
                    @"\IslandShape.json",
                    @"\Noise.json",
                    @"\FallOff.json",
                };
            }
        }
        
        /// <summary>
        /// Array containing all files path for the map as a whole
        /// </summary>
        public readonly string[] DelaunayFilesPath
        {
            get
            {
                return new string[]
                {
                    @"\triangles.json",
                    @"\halfedges.json",
                    @"\hull.json",
                };
            }
        }

        //triangles are the same for each chunk (since it only define the draw order of the mesh)
        private const string ChunksTrianglesFile = @"\Triangles.json";
        private const string ChunksSharedVertexFile = @"\SharedVertices.json";
        /// <summary>
        /// Array containing all files path for each chunk
        /// </summary>
        public readonly string[] ChunkFilesPath
        {
            get
            {
                return new string[]
                {
                    @"\VerticesPosition.json",
                    @"\VerticesCellIndex.json",
                    @"\Uvs.json",
                    @"\Noise.json",
                    @"\PoissonDiscPosition.json",
                    @"\PoissonDiscCellIndex.json",
                };
            }
        }
        #endregion FILES
    }
    
    [Flags]
    public enum DelaunayFiles : int
    {
        triangles = 0,
        halfedges = 1,
        hull = 2,
    }

    [Flags]
    public enum ChunkFiles : int
    {
        VerticesPos = 0,
        VerticesCellIndex = 1,
        Uvs = 2,
        PerlinNoise = 3,
    }
    [Flags]
    public enum FullMapFiles : int
    {
        VerticesPos = 0,
        VerticesCellIndex = 1,
        PoissonDiscPos = 2,
        PoissonDiscId = 3,
        Uvs = 4,
        Voronoi = 5,
        Island = 6,
        Noise = 7,
        FallOff = 8,
    }
}
