using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KaizerWaldCode.TerrainGeneration;
using KaizerWaldCode.TerrainGeneration.KwSystem;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWaldCode
{
    public class MapGenerationDirectories
    {
        static string SaveFilePath = "DefaultSaveName";
        //static readonly string SaveFilePath = "DefaultSaveName";

        const string Folder_SaveFiles= "SaveFiles";
        const string Folder_TerrainGen = "TerrainGeneration";
        const string Folder_MapDatas = "MapDatas";
        const string Folder_DelaunayDatas = "Delaunay";
        const string Folder_Chunks = "Chunks";
        const string Folder_ChunksSharedDatas = "ChunksSharedDatas";
        
        //FILES
        static readonly string[] Files_Map;
        static readonly string[] Files_Delaunay;
        static readonly string[] Files_Chunk;
        
        //triangles are the same for each chunk (since it only define the draw order of the mesh)
        const string File_ChunksSharedTriangles = @"SharedTriangles.json";
        const string File_ChunksSharedVertex = @"SharedVertices.json";
        
        //public string SelectedSave;
        public MapGenerationDirectories(in string path)
        {
            SaveFilePath = path;
        }
        
        //allow to init readonly arrays
        static MapGenerationDirectories()
        {
            Files_Map = new string[]
            {
                @"VerticesPosition.json",
                @"VerticesCellIndex.json",
                @"PoissonDiscPosition.json",
                @"PoissonDiscCellIndex.json",
                @"Uvs.json",
                @"Voronoi.json",
                @"IslandShape.json",
                @"Noise.json",
                @"FallOff.json",
            };
            
            Files_Delaunay = new string[]
            {
                @"Triangles.json",
                @"Halfedges.json",
                @"Hull.json",
            };
            
            Files_Chunk = new string[]
            {
                @"VerticesPosition.json",
                @"VerticesCellIndex.json",
                @"Uvs.json",
                @"Noise.json",
                @"PoissonDiscPosition.json",
                @"PoissonDiscCellIndex.json",
            };
            
            
        }
        
        //========================
        //FOLDERS METHODS
        //========================
        #region GET FOLDER METHODS
        //Get Base Save Folder : Save Root -> Save Files
        public static readonly string GetFolder_SelectedSave = Path.Combine(Application.persistentDataPath, Folder_SaveFiles, SaveFilePath);
        //Get Terrain Generation Folder : Save Root -> Save Files -> TerrainGenerationDatas
        public static readonly string GetFolder_TerrainGeneration = Path.Combine(GetFolder_SelectedSave, Folder_TerrainGen);
        //Get Map Datas Folder : Save Root -> Save Files -> TerrainGenerationDatas -> MapDatas
        public static readonly string GetFolder_MapDatas = Path.Combine(GetFolder_TerrainGeneration, Folder_MapDatas);
        //Get Delaunay Folder : Save Root -> Save Files -> TerrainGenerationDatas -> MapDatas -> DelaunayDatas
        public static readonly string GetFolder_Delaunay = Path.Combine(GetFolder_MapDatas, Folder_DelaunayDatas);
        //Get Chunks Folder : Save Root -> Save Files -> TerrainGenerationDatas -> Chunks
        public static readonly string GetFolder_Chunks = Path.Combine(GetFolder_TerrainGeneration, Folder_Chunks);
        //Get Chunks Folder : Save Root -> Save Files -> TerrainGenerationDatas -> Chunks -> ChunksSharedDatas
        public static readonly string GetFolder_ChunksSharedDatas = Path.Combine(GetFolder_Chunks, Folder_ChunksSharedDatas);
        
        /// <summary>
        /// Get Individual Chunk named from their position in the grid
        /// </summary>
        /// <param name="x">X position of the Chunk</param>
        /// <param name="y">Y position of the Chunk</param>
        /// <returns>full path to the chunk (in "Chunks folder")</returns>
        public static string GetFolder_ChunkXY(in int x, in int y) => Path.Combine(GetFolder_Chunks, $"ChunkX{x}Y{y}");

        #endregion GET FOLDER METHODS
        
        //========================
        //FILES METHODS
        //========================
        #region FILES
        
        //Delaunay Datas
        public static string GetFile_DelaunayAt(in DelaunayFiles file) => Path.Combine(GetFolder_Delaunay, Files_Delaunay[(int)file]);
        //Map Datas
        public static string GetFile_MapAt(in FullMapFiles file) => Path.Combine(Folder_MapDatas, Files_Map[(int)file]);
        
        
        /// <summary>
        /// Return the path to a given chunk (depending on x,y value entered)
        /// </summary>
        /// <param name="x">X Position of the chunk</param>
        /// <param name="y">Y position of the chunk</param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFile_ChunkXYAt(in int x, in int y, in ChunkFiles file) => Path.Combine(GetFolder_ChunkXY(x, y), Files_Chunk[(int)file]);
        //Shared Chunks Datas
        public static readonly string GetFile_ChunksSharedTriangles = Path.Combine(GetFolder_ChunksSharedDatas, File_ChunksSharedTriangles);
        public static readonly string GetFile_ChunksSharedVertex = Path.Combine(GetFolder_ChunksSharedDatas, File_ChunksSharedVertex);
        
        #endregion FILES
    }
}
