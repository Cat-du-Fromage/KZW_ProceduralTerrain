using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KaizerWaldCode.TerrainGeneration
{
    public class DataPersistanceSystem : MonoBehaviour
    {
        void OnEnable() => Load();

        void OnDisable() => Save();

        void Load()
        {
            foreach (ISaveState p in FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveState>())
            {
                p.Load();
            }
        }

        void Save()
        {
            foreach (ISaveState p in FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveState>())
            {
                p.Save();
            }
        }
    }

    internal interface ISaveState
    {
        void Save();
        void Load();
    }
}
