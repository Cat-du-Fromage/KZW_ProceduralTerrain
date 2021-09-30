using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.TerrainGeneration;
using KaizerWaldCode.TerrainGeneration.Data;
using UnityEngine;

namespace KaizerWaldCode
{
    [Flags]
    enum State_MapDatas : int
    {
        Vertices     = 0,
        RandomPoints = 1,
        End          = Int32.MaxValue, 
    }
    
    public class MapDatasStateMachine : IStateMachine
    {
        public readonly int numStates = Enum.GetValues(typeof(State_MapDatas)).Length - 1;
        Queue<State_MapDatas> states;
        
        //States
        VerticesState verticesState;
        RandomPointsState randomPointsState;

        public MapDatasStateMachine(in SettingsData mapSettings)
        {
            verticesState = new VerticesState(mapSettings);
            randomPointsState = new RandomPointsState(mapSettings);
        }

        public void InitializeStateMachine()
        {
            states = new Queue<State_MapDatas>(numStates);
            foreach (State_MapDatas state in Enum.GetValues(typeof(State_MapDatas)))
            {
                if (state == State_MapDatas.End) break;
                states.Enqueue(state);
            }
            StateMachineStart();
        }

        void StateMachineStart()
        {
            for (int i = 0; i < numStates; i++)
            {
                ChangeState(states.Dequeue());
            }
        }
        
        void ChangeState(State_MapDatas current)
        {
            switch(current)
            {
                case State_MapDatas.Vertices:
                    verticesState.DoState();
                    break;
                case State_MapDatas.RandomPoints:
                    randomPointsState.DoState();
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
