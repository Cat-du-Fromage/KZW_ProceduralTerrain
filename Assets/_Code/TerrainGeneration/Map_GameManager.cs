using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.TerrainGeneration
{
    public class Map_GameManager : MonoBehaviour
    {
        public static Map_GameManager Instance;
        public GenerationState State;

        void Awake()
        {
            Instance = this;
        }

        // Update is called once per frame
        void UpdateGameState(GenerationState newState)
        {
            State = newState;

            switch (newState)
            {
                case GenerationState.VertexGrid:
                    break;
                case GenerationState.PoissonDiscSamples:
                    break;
                case GenerationState.VoronoiAttribution:
                    break;
                case GenerationState.StoreValues:
                    break;
                case GenerationState.MapFinished:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }
    }

    public enum GenerationState
    {
        VertexGrid,
        PoissonDiscSamples,
        VoronoiAttribution,
        StoreValues,
        MapFinished
    }
}
