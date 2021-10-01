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
        End          = Int32.MaxValue, 
    }
    
    public class MapDatasStateMachine : IStateMachine<EStateMapDatas>
    {
        public readonly int numStates = Enum.GetValues(typeof(EStateMapDatas)).Length - 1;
        public List<EStateMapDatas> States { get; set; }
        
        //States
        VerticesState verticesState;
        RandomPointsState randomPointsState;
        VoronoiState voronoiState;

        public MapDatasStateMachine(in SettingsData mapSettings)
        {
            verticesState = new VerticesState(in mapSettings);
            randomPointsState = new RandomPointsState(in mapSettings);
            voronoiState = new VoronoiState(in mapSettings);
            InitializeStateMachine();
        }

        public void InitializeStateMachine()
        {
            Debug.Log($"Init OK");
            States = new List<EStateMapDatas>(numStates);
            
            foreach (EStateMapDatas state in Enum.GetValues(typeof(EStateMapDatas)))
            {
                Debug.Log($"Launch {state}");
                if (state == EStateMapDatas.End) continue;
                States.Add(state);
            }
            StateMachineStart();
        }

        void StateMachineStart()
        {
            for (int i = 0; i < numStates; i++)
            {
                ChangeState(States[i]);
            }
        }
        
        void ChangeState(EStateMapDatas current)
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
