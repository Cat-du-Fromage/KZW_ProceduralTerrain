using System.Collections;
using System.Collections.Generic;
using System.IO;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.TerrainGeneration.KwSystem;
using UnityEngine;

using static KaizerWaldCode.Utils.KWmath;
using static Unity.Mathematics.math;
using dir = KaizerWaldCode.TerrainGeneration.Directories_MapGeneration;
using static KaizerWaldCode.KWSerialization.BinarySerialization;

namespace KaizerWaldCode.TerrainGeneration
{
    public class TerrainGenerationDirectoriesTree
    {
        SettingsData mapSettings;
        
        public TerrainGenerationDirectoriesTree(in SettingsData mapSettings)
        {
            this.mapSettings = mapSettings;
            GetOrCreateDirectories();
        }
        
        private void GetOrCreateDirectories()
        {
            //dir.SelectedSave = folderName;
            Debug.Log(dir.ToString());
            
            //"Directory.CreateDirectory" create all missing directory in the path(does not create a duplicate if already exist)
            if (!Directory.Exists(dir.GetFolder_TerrainGeneration)) { Directory.CreateDirectory(dir.GetFolder_TerrainGeneration); }
            
            if (!Directory.Exists(dir.GetFolder_MapDatas)) 
            { 
                Directory.CreateDirectory(dir.GetFolder_MapDatas);
                CreateFullMapFiles(false);
            }
            else
            {
                CreateFullMapFiles(true);
            }

            if (!Directory.Exists(dir.GetFolder_Delaunay))
            {
                Directory.CreateDirectory(dir.GetFolder_Delaunay);
                CreateDelaunayDatasFiles(false);
            }
            else
            {
                CreateDelaunayDatasFiles(true);
            }
            
            //CHunks Shared Data Directory
            if (!Directory.Exists(dir.GetFolder_ChunksSharedDatas)) { Directory.CreateDirectory(dir.GetFolder_ChunksSharedDatas); }
            CreateTrianglesFile();
            CreateVertexFile();
            
            //individual Chunks Folder
            //string chunkPath;
            for (int i = 0; i < sq(mapSettings.NumChunk); i++)
            {
                int ChunkPosY = (int)floor(i / (float)mapSettings.NumChunk);
                int ChunkPosX = i - (ChunkPosY * mapSettings.NumChunk);

                if (!Directory.Exists(dir.GetFolder_ChunkXY(ChunkPosX, ChunkPosY))) 
                {
                    Directory.CreateDirectory(dir.GetFolder_ChunkXY(ChunkPosX, ChunkPosY));
                    CreateChunksFiles(ChunkPosX, ChunkPosY, false);
                }
                else
                {
                    CreateChunksFiles(ChunkPosX, ChunkPosY, true);
                }
            }
        }
        
        private void CreateChunksFiles(in int x, in int y, bool checkExist = true)
        {
            if(checkExist)
                for (int j = 0; j < dir.Files_Chunk.Length; j++)
                {
                    if (File.Exists(dir.GetFile_ChunkXYAt(x, y, j))) continue;
                    CreateFile(dir.GetFile_ChunkXYAt(x, y, j));
                }
            else
            {
                for (int j = 0; j < dir.Files_Chunk.Length; j++)
                {
                    CreateFile(dir.GetFile_ChunkXYAt(x, y, j));
                }
            }
        }

        private void CreateFullMapFiles(bool checkExist = true)
        {
            if (checkExist)
                for (int j = 0; j < dir.Files_Map.Length; j++)
                {
                    if (File.Exists(dir.GetFile_MapAt(j))) continue;
                    
                    CreateFile(dir.GetFile_MapAt(j));
                }
            else
            {
                for (int j = 0; j < dir.Files_Map.Length; j++)
                {
                    CreateFile(dir.GetFile_MapAt((MapFiles)j));
                }
            }
        }
        
        private void CreateDelaunayDatasFiles(bool checkExist = true)
        {
            if (checkExist)
                for (int j = 0; j < dir.Files_Delaunay.Length; j++)
                {
                    if (File.Exists(dir.GetFile_DelaunayAt((DelaunayFiles)j))) continue;
                    CreateFile(dir.GetFile_DelaunayAt((DelaunayFiles)j));
                }
            else
            {
                for (int j = 0; j < dir.Files_Delaunay.Length; j++)
                {
                    CreateFile(dir.GetFile_DelaunayAt((DelaunayFiles)j));
                }
            }
        }

        private void CreateTrianglesFile(bool checkExist = true)
        {
            if (checkExist)
            {
                if ( File.Exists(dir.GetFile_ChunksSharedTriangles()) ) return;
                CreateFile(dir.GetFile_ChunksSharedTriangles());
            }
            else
            {
                CreateFile(dir.GetFile_ChunksSharedTriangles());
            }
        }
        
        private void CreateVertexFile(bool checkExist = true)
        {
            if (checkExist)
            {
                if ( File.Exists(dir.GetFile_ChunksSharedVertex()) ) return;
                CreateFile(dir.GetFile_ChunksSharedVertex());
            }
            else
            {
                CreateFile(dir.GetFile_ChunksSharedVertex());
            }
        }
    }
}
