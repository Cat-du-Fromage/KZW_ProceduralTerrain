using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KaizerWaldCode.TerrainGeneration;
using KaizerWaldCode.TerrainGeneration.KwSystem;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Collections.LowLevel.Unsafe.UnsafeUtility;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KaizerWaldCode.TerrainGeneration
{
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
    public enum IslandFiles : int
    {
        Indices = 0,
        PointsDistance = 1,
    }
    
    [Flags]
    public enum MapFiles : int
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
        PointsDistance = 9,
    }
    public class Directories_MapGeneration
    {
        public static string ToString()
        {
            return SaveFilePath;
        }

        //bool SaveSelected = false; // will need to change value when entering Main Menu
        static string SaveFilePath = "DefaultSaveName";

        const string Folder_SaveFiles = "SaveFiles";
        const string Folder_TerrainGen = "TerrainGeneration";
        const string Folder_MapDatas = "MapDatas";
        const string Folder_DelaunayDatas = "Delaunay";
        const string Folder_Chunks = "Chunks";
        const string Folder_ChunksSharedDatas = "ChunksSharedDatas";
        //triangles are the same for each chunk (since it only define the draw order of the mesh)
        const string File_ChunksSharedTriangles = @"\SharedTriangles.json";
        const string File_ChunksSharedVertex = @"\SharedVertices.json";

        //FILES
        public static readonly string[] Files_Map;
        public static readonly string[] Files_Delaunay;
        public static readonly string[] Files_Chunk;

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        static void OnEnterPlaymodeInEditor(EnterPlayModeOptions options)
        {
            if (options.HasFlag(EnterPlayModeOptions.DisableDomainReload))
            {
                SaveFilePath = "DefaultSaveName";
                Debug.Log($"Entering PlayMode = {SaveFilePath}");
            }
        }
#endif

        //public string SelectedSave;
        public Directories_MapGeneration(in string path)
        {
            if (SaveFilePath == "DefaultSaveName")
            {
                SaveFilePath = path;
            }
            
            Debug.Log($"InitSaveFIle Name {SaveFilePath}");
        }

        //allow to init readonly arrays
        static Directories_MapGeneration()
        {
            Files_Map = new string[]
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
                @"\PointsDistance.json",
            };

            Files_Delaunay = new string[]
            {
                @"\Triangles.json",
                @"\Halfedges.json",
                @"\Hull.json",
            };

            Files_Chunk = new string[]
            {
                @"\VerticesPosition.json",
                @"\VerticesCellIndex.json",
                @"\Uvs.json",
                @"\Noise.json",
                @"\PoissonDiscPosition.json",
                @"\PoissonDiscCellIndex.json",
            };


        }

        //========================
        //FOLDERS METHODS
        //========================

        #region GET FOLDER METHODS

        //Get Base Save Folder : Save Root -> Save Files
        public static string GetFolder_SelectedSave { get => Path.Combine(Application.persistentDataPath, Folder_SaveFiles, SaveFilePath); }

        //Get Terrain Generation Folder : Save Root -> Save Files -> TerrainGenerationDatas
        public static string GetFolder_TerrainGeneration { get => Path.Combine(GetFolder_SelectedSave, Folder_TerrainGen); }

        //Get Map Datas Folder : Save Root -> Save Files -> TerrainGenerationDatas -> MapDatas
        public static string GetFolder_MapDatas { get => Path.Combine(GetFolder_TerrainGeneration, Folder_MapDatas); }

        //Get Delaunay Folder : Save Root -> Save Files -> TerrainGenerationDatas -> MapDatas -> DelaunayDatas
        public static string GetFolder_Delaunay { get => Path.Combine(GetFolder_MapDatas, Folder_DelaunayDatas); }
        
        //Get Chunks Folder : Save Root -> Save Files -> TerrainGenerationDatas -> Chunks
        public static string GetFolder_Chunks { get => Path.Combine(GetFolder_TerrainGeneration, Folder_Chunks); }
        //Get Chunks Folder : Save Root -> Save Files -> TerrainGenerationDatas -> Chunks -> ChunksSharedDatas
        public static string GetFolder_ChunksSharedDatas { get => Path.Combine(GetFolder_Chunks, Folder_ChunksSharedDatas); }
        
        /// <summary>
        /// Get Individual Chunk named from their position in the grid
        /// </summary>
        /// <param name="x">X position of the Chunk</param>
        /// <param name="y">Y position of the Chunk</param>
        /// <returns>full path to the chunk (in "Chunks folder")</returns>
        public static string GetFolder_ChunkXY(in int x, in int y) => Path.Combine(GetFolder_Chunks, $"ChunkX{x}Y{y}");
        public static string GetFolder_ChunkXY(in int2 pos) => Path.Combine(GetFolder_Chunks, $"ChunkX{pos.x}Y{pos.y}");
        #endregion GET FOLDER METHODS
        
        //========================
        //FILES METHODS
        //========================
        #region FILES
        //CAREFUL Path.Combine DONT WORK for files
        //Delaunay Datas
        public static string GetFile_DelaunayAt(in DelaunayFiles file) => $"{GetFolder_Delaunay}{Files_Delaunay[EnumToInt(file)]}";
        public static string GetFile_DelaunayAt(in int file) => $"{GetFolder_Delaunay}{Files_Delaunay[file]}";
        //Map Datas
        public static string GetFile_MapAt(in MapFiles file) => $"{GetFolder_MapDatas}{Files_Map[EnumToInt(file)]}";
        public static string GetFile_MapAt(in int file) => $"{GetFolder_MapDatas}{Files_Map[file]}";
        
        /// <summary>
        /// Return the path to a given chunk (depending on x,y value entered)
        /// </summary>
        /// <param name="x">X Position of the chunk</param>
        /// <param name="y">Y position of the chunk</param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFile_ChunkXYAt(in int x, in int y, in ChunkFiles file) => $"{GetFolder_ChunkXY(x, y)}{ Files_Chunk[EnumToInt(file)]}";
        public static string GetFile_ChunkXYAt(in int2 pos, in ChunkFiles file) => $"{GetFolder_ChunkXY(pos)}{ Files_Chunk[EnumToInt(file)]}";
        public static string GetFile_ChunkXYAt(in int x, in int y, in int file) => $"{GetFolder_ChunkXY(x, y)}{ Files_Chunk[file]}";
        public static string GetFile_ChunkXYAt(in int2 pos, in int file) => $"{GetFolder_ChunkXY(pos)}{ Files_Chunk[file]}";
        //Shared Chunks Datas
        public static string GetFile_ChunksSharedTriangles() => $"{GetFolder_ChunksSharedDatas}{File_ChunksSharedTriangles}";
        public static string GetFile_ChunksSharedVertex() => $"{GetFolder_ChunksSharedDatas}{File_ChunksSharedVertex}";
            
        
        #endregion FILES
    }
}
