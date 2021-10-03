using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.TerrainGeneration;
using KaizerWaldCode.TerrainGeneration.Data;
using UnityEngine;

namespace KaizerWaldCode.TerrainGeneration
{
    [Flags]
    public enum EStateMapDatas : int
    {
        Vertices     = 0,
        RandomPoints = 1,
        Voronoi      = 2,
        Uvs          = 3,
        End          = int.MaxValue, 
    }
    
    public sealed class MapDatasStateMachine : IFiniteStateMachine<EStateMapDatas>
    {
        readonly int numStates = Enum.GetValues(typeof(EStateMapDatas)).Length - 1;
        public List<EStateMapDatas> States { get; set; }
        
        //States
        VerticesState verticesState;
        RandomPointsState randomPointsState;
        VoronoiState voronoiState;
        UvsState uvsState;

        public MapDatasStateMachine(in SettingsData mapSettings)
        {
            verticesState = new VerticesState(in mapSettings);
            randomPointsState = new RandomPointsState(in mapSettings);
            voronoiState = new VoronoiState(in mapSettings);
            uvsState = new UvsState(in mapSettings);
            InitializeStateMachine();
            StateMachineStart();
        }

        public void InitializeStateMachine()
        {
            States = new List<EStateMapDatas>(numStates);
            
            foreach (EStateMapDatas state in Enum.GetValues(typeof(EStateMapDatas)))
            {
                if (state == EStateMapDatas.End) break;
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
        
        public void ChangeState(EStateMapDatas current)
        {
            switch(current)
            {
                case EStateMapDatas.Vertices:
                    verticesState.DoState();
                    break;
                case EStateMapDatas.RandomPoints:
                    randomPointsState.DoState();
                    break;
                case EStateMapDatas.Voronoi:
                    voronoiState.DoState();
                    break;
                case EStateMapDatas.Uvs:
                    uvsState.DoState();
                    break;
                case EStateMapDatas.End:
                    break;
                default:
                    break;
            };
        }

        /*
        State_MapDatas ChangeState(State_MapDatas current) => 
        (current) switch
        {
            (State_MapDatas.Vertices) => State_MapDatas.RandomPoints,
            (State_MapDatas.RandomPoints) => State_MapDatas.End,
        };
        */
    }
}
