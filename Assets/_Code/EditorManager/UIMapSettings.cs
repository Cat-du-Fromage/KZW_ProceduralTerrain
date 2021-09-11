using KaizerWaldCode.TerrainGeneration.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace KaizerWaldCode
{
    public class UIMapSettings : MonoBehaviour
    {
        [HideInInspector]
        public bool NewGame;
        [HideInInspector]
        public SettingsData MapSettings;
        [HideInInspector]
        public string FolderName;
        public AssetReference MapSystemPrefab;
    }
}
