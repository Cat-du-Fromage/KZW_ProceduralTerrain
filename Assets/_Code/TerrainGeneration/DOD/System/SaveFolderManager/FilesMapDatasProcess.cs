using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using static KaizerWaldCode.KWSerialization.BinarySerialization;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    /// <summary>
    /// Files Map Datas Process
    /// Check/Create all binary Files related to the map generation
    /// </summary>
    public partial class MapSystem : MonoBehaviour
    {
        // dir : reference to MapDirectories

        private void GetOrCreateFiles(in string folderName)
        {

        }

        private void CreateChunksFiles(in int x, in int y, bool checkExist = true)
        {
            if(checkExist)
                for (int j = 0; j < dir.ChunkFilesPath.Length; j++)
                {
                    if (File.Exists(dir.GetChunkFileAt(x, y, j))) continue;
                    CreateFile(dir.GetChunkFileAt(x, y, j));
                }
            else
            {
                for (int j = 0; j < dir.ChunkFilesPath.Length; j++)
                {
                    CreateFile(dir.GetChunkFileAt(x, y, j));
                }
            }
        }

        private void CreateFullMapFiles(bool checkExist = true)
        {
            if (checkExist)
                for (int j = 0; j < dir.FullMapFilesPath.Length; j++)
                {
                    if (File.Exists(dir.GetFullMapFileAt(j))) continue;
                    CreateFile(dir.GetFullMapFileAt(j));
                }
            else
            {
                for (int j = 0; j < dir.FullMapFilesPath.Length; j++)
                {
                    CreateFile(dir.GetFullMapFileAt(j));
                }
            }
        }

        private void CreateTrianglesFile(bool checkExist = true)
        {
            if (checkExist)
            {
                if ( File.Exists(dir.GetChunksTriangleFile()) ) return;
                CreateFile(dir.GetChunksTriangleFile());
            }
            else
            {
                CreateFile(dir.GetChunksTriangleFile());
            }
        }
        
        private void CreateVertexFile(bool checkExist = true)
        {
            if (checkExist)
            {
                if ( File.Exists(dir.GetChunksSharedVertexFile()) ) return;
                CreateFile(dir.GetChunksSharedVertexFile());
            }
            else
            {
                CreateFile(dir.GetChunksSharedVertexFile());
            }
        }
    }



}
