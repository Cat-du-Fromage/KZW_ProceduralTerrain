using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.TerrainGeneration;
using KaizerWaldCode.TerrainGeneration.Data;
using UnityEngine;

namespace KaizerWaldCode.TerrainGeneration
{
    [Flags]
    public enum EStateIsland : int
    {
        Shape               = 0,
        PointsDstFromIsland = 1,
        End                 = int.MaxValue, 
    }

    public sealed class IslandStateMachine : IFiniteStateMachine<EStateIsland>
    {
        readonly int numStates = Enum.GetValues(typeof(EStateIsland)).Length - 1;
        
        public List<EStateIsland> States { get; set; }
        
        //States
        IslandShapeState islandShapeState;
        PointsDstFromIslandState pointsDstFromIslandState;

        public IslandStateMachine(in SettingsData mapSettings)
        {
            islandShapeState = new IslandShapeState(in mapSettings);
            pointsDstFromIslandState = new PointsDstFromIslandState(in mapSettings);
            InitializeStateMachine();
            StateMachineStart();
        }
        
        public void InitializeStateMachine()
        {
            States = new List<EStateIsland>(numStates);
            foreach (EStateIsland state in Enum.GetValues(typeof(EStateIsland)))
            {
                if (state == EStateIsland.End) break; //we may want to return true for the game manager later
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

        public void ChangeState(EStateIsland current)
        {
            switch(current)
            {
                case EStateIsland.Shape:
                    islandShapeState.DoState();
                    break;
                case EStateIsland.PointsDstFromIsland:
                    pointsDstFromIslandState.DoState();
                    break;
                case EStateIsland.End:
                    break;
                default:
                    break;
            };
        }
    }
}
