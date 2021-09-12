using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KaizerWaldCode.TerrainGeneration.KwSystem
{
    /// <summary>
    /// Files Map Datas Process
    /// Check/Create all binary Files related to the map generation
    /// </summary>
    public partial class MapSystem : MonoBehaviour
    {
        private MapFiles files;

        private void GetOrCreateFiles(in string folderName)
        {

        }
    }

    [Serializable]
    struct MapFiles
    {
        public string FullMapDatasPath;

        public readonly string[] FullMap
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
        private const string ChunksTriangles = @"\Triangles.txt";

        public readonly string[] Chunk
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

        public readonly string GetFullMapFile(in string mapDataPath, in int file)
        {
            return $"{mapDataPath}{FullMap[file]}";
        }

        public readonly string GetChunkFile(in string chunkPath, ChunkFiles file)
        {
            return $"{chunkPath}{Chunk[(int)file]}";
        }
    }

    public enum ChunkFiles
    {
        VerticesPos = 0,
        VerticesCellIndex = 1,
        Uvs = 2,
    }

    public enum EFullMapFiles
    {
        VerticesPos = 0,
        VerticesCellIndex = 1,
        Uvs = 2,
    }
}
