using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.TerrainGeneration.Data;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace KaizerWaldCode.TerrainGeneration
{
    [Flags]
    public enum EStateChunkSlicer : int
    {
        CreateChunks         = 0,
        VerticesSlice        = 1,
        TrianglesSlice       = 2,
        UvsSlice             = 3,
        BuildMeshes          = 4,
        End                  = int.MaxValue,
    }
    
    public sealed class ChunkSlicerStateMachine : IFiniteStateMachine<EStateChunkSlicer>
    {
        SettingsData mapSettings;
        ChunksData[] chunksData;
        public List<EStateChunkSlicer> States { get; set; }
        readonly int numStates = Enum.GetValues(typeof(EStateChunkSlicer)).Length - 1;
        //States
        CreateChunksState createChunksState;
        VerticesSliceState verticesSliceState;
        SharedTrianglesState sharedTrianglesState;
        UvsSliceState uvsSliceState;
        BuildMeshesState buildMeshesState;
        public ChunkSlicerStateMachine(in SettingsData mapSettings)
        {
            createChunksState = new CreateChunksState(in mapSettings);
            verticesSliceState = new VerticesSliceState(in mapSettings);
            sharedTrianglesState = new SharedTrianglesState(in mapSettings);
            uvsSliceState = new UvsSliceState(in mapSettings);
            buildMeshesState = new BuildMeshesState(in mapSettings);
            this.mapSettings = mapSettings;
            InitializeStateMachine();
            StateMachineStart();
        }
        
        public void InitializeStateMachine()
        {
            States = new List<EStateChunkSlicer>(numStates);
            
            foreach (EStateChunkSlicer state in Enum.GetValues(typeof(EStateChunkSlicer)))
            {
                if (state == EStateChunkSlicer.End) break;
                States.Add(state);
            }
        }

        public void StateMachineStart()
        {
            for (int i = 0; i < numStates; i++)
            {
                ChangeState(States[i]);
            }
        }

        public void ChangeState(EStateChunkSlicer current)
        {
            switch(current)
            {
                case EStateChunkSlicer.CreateChunks:
                    createChunksState.DoState();
                    GetChunksData();
                    break;
                case EStateChunkSlicer.VerticesSlice:
                    verticesSliceState.chunksData = chunksData;
                    verticesSliceState.DoState();
                    break;
                case EStateChunkSlicer.TrianglesSlice:
                    sharedTrianglesState.DoState();
                    break;
                case EStateChunkSlicer.UvsSlice:
                    uvsSliceState.chunksData = chunksData;
                    uvsSliceState.DoState();
                    break;
                case EStateChunkSlicer.BuildMeshes:
                    buildMeshesState.chunksData = chunksData;
                    buildMeshesState.DoState();
                    break;
                default:
                    break;
            };
        }
        
        void GetChunksData()
        {
            GameObject[] chunkObjs = GameObject.FindGameObjectsWithTag("Map Chunk");
            chunksData = new ChunksData[chunkObjs.Length];
            for (int i = 0; i < chunkObjs.Length; i++)
            {
                chunksData[i] = chunkObjs[i].GetComponent<ChunksData>();
            }
        }
    }
}
