using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.KWSerialization;
using KaizerWaldCode.TerrainGeneration.Data;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using Debug = UnityEngine.Debug;
using System.Collections.Specialized;

namespace KaizerWaldCode.TerrainGeneration.KwEntity
{
    /*
    [Flags]
    public enum MapState
    {
        Init = 0,
        Vertex = 1,
        Poisson = 2,
        Voronoi = 3,
        Island = 4,
        Noise = 5,
        FallOff = 6,
        End = 10
    }
    */
    public class MapEntity : MonoBehaviour
    {
        [SerializeField] private bool newGame;
        [Min(1)]
        [SerializeField] private int chunkSize;
        [Min(1)]
        [SerializeField] private int numChunk;
        [Range(2, 10)]
        [SerializeField] private int pointPerMeter;

        public MapSettingsData Settings { get; private set; }

        private string filePath;

        //private MapState state;

        private BitField32 bitfield;
        

        void testBitField()
        {
            BitArray testba = new BitArray(8);
            BitVector32 bitVector32;

            bitfield = new BitField32();
            bitfield.SetBits(16, true, 16);

            //bitfield.GetBits(1, 32);
            Debug.Log($"{bitfield.GetBits(0, 16)} {bitfield.GetBits(16, 16)} Binary {Convert.ToString(bitfield.GetBits(16, 16), 2)}" );

            Debug.Log($"CountTrailingZeros {bitfield.CountBits()}CountLeadingZeros {bitfield.CountLeadingZeros()}");

            uint a = 0b00000001;
            uint b = 1;
            Debug.Log($"{((int)a)}  et  {a.ToString()}");
            a |= bitfield.GetBits(16, 16) & 23u;
            Debug.Log(Convert.ToString(a,2));
        }

        void Awake()
        {

            testBitField();

            
            filePath = $"{Application.persistentDataPath}/MapSettings/{nameof(MapEntity)}";
            if (JsonSerialization.SaveExist(filePath) && !newGame)
            {
                Settings = JsonSerialization.Load<SettingsData>(filePath);
            }
            else
            {
                Settings = new MapSettingsData()
                {
                    ChunkSize = max(1, chunkSize),
                    NumChunk = max(1, numChunk),
                    PointPerMeter = MinMax(2, 10),

                    MapSize = chunkSize * numChunk,
                    PointSpacing = 1f / (pointPerMeter - 1f),
                    ChunkPointPerAxis = (chunkSize * pointPerMeter) - (chunkSize - 1),
                    MapPointPerAxis = (numChunk * chunkSize) * pointPerMeter - (numChunk * chunkSize - 1),
                };
            }
        }

        void OnDisable()
        {
            SettingsData dataToSave = Settings;
            JsonSerialization.Save(filePath, dataToSave);
        }

        void Checker()
        {
            BitField32 state = new BitField32();

            //VerticesPosition == Not(FILE Exist || NumVertices == MapPointPerAxis^2)
            //Si Not FILE Exist => create file
            //Si Not (CountVerticesInFile == MapPointPerAxis^2)

            //VerticesCellIndex

            //PoissonDiscPosition
            //PoissonDiscCellIndex

            //SI New Map on rest a 0 OU si BitField32 full 1 pareil
        }
        /*
        void StateMachine(MapState newstate)
        {
            switch (newstate)
            {
                case MapState.Init:
                    break;
                case MapState.Vertex:
                    break;
                case MapState.Poisson:
                    break;
                case MapState.Voronoi:
                    break;
                case MapState.Island:
                    break;
                case MapState.Noise:
                    break;
                case MapState.FallOff:
                    break;
                case MapState.End:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newstate), newstate, null);
            }
        }
        */
    }
}
