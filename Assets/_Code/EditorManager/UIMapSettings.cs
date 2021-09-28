using System.Collections;
using System.Collections.Generic;
using System.IO;
using KaizerWaldCode.TerrainGeneration;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.TerrainGeneration.KwEntity;
using KaizerWaldCode.TerrainGeneration.KwSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif

using static KaizerWaldCode.Utils.AddressablesUtils;
using static Unity.Mathematics.math;

namespace KaizerWaldCode.KwEditor
{
    /*
    public class UIMapSettings : MonoBehaviour
    {
        [HideInInspector]
        public bool NewGame;
        [HideInInspector]
        public SettingsData MapSettings;
        [HideInInspector]
        public string FolderName;
        public AssetReference MapSystemPrefab;
        
        public DebbugingUI ui;
    }
    */
    /// <summary>
    /// What is displayed on the editor
    /// </summary>
    public class UIMapSettings : MonoBehaviour
    {
        [HideInInspector]
        public uint Seed;
        [HideInInspector]
        public bool NewGame;
        [HideInInspector]
        public SettingsData MapSettings;
        [HideInInspector]
        public PerlinNoiseData NoiseSettings;
        [HideInInspector]
        public string FolderName;
        public AssetReference MapSystemPrefab;
        
        public DebbugingUI ui;
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(UIMapSettings))]
    [CanEditMultipleObjects]
    public class EditorMapSettings : Editor
    {
        //Bool SHOW SETTINGS
        private AnimBool showNoise;
        private bool foldOutNoise = true;
        private string noiseStatus = "Perlin Noise Settings";
        
        //private bool newGame;
        private SerializedProperty mapSettings;
        private SerializedProperty newGame;
        private SerializedProperty folderName;
        private SerializedProperty mapSystemPrefab;

        //Save Files
        public string[] files;
        bool[] pos = new bool[3] { false, false, false };

        //Internal Private fields
        private string savesFolder;
        private DirectoryInfo directory;
        private int numSaves;

        void OnEnable()
        {
            showNoise = new AnimBool(true);
            showNoise.valueChanged.AddListener(Repaint); // without this there is a strange latency between toggle performed and hide/show
            
            savesFolder = $"{Application.persistentDataPath}/SaveFiles/";
            InitPropreties();

            if (!Directory.Exists(savesFolder)) Directory.CreateDirectory(savesFolder);

            DirectoryAndNumSaves(out numSaves, out directory);
            files = new string[numSaves];
            GetFilesName(savesFolder, ref files, numSaves, directory);
        }

        void OnValidate()
        {
            directory = new DirectoryInfo(savesFolder);
            if (numSaves != directory.GetDirectories().Length)
            {
                numSaves = directory.GetDirectories().Length;
                files = new string[numSaves];
            }

            GetFilesName(savesFolder, ref files, numSaves, directory);
        }

        void MapSettingGUI(UIMapSettings uiSettings)
        {
            EditorGUILayout.PropertyField(folderName, new GUIContent("Save Name"));
            GUI.enabled = false;
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(mapSettings, new GUIContent("Map Settings"));
            EditorGUILayout.Space(2);
            GUI.enabled = true;
            EditorGUILayout.PropertyField(mapSettings.FindPropertyRelative("ChunkSize"),new GUIContent("Chunk Size"));
            EditorGUILayout.PropertyField(mapSettings.FindPropertyRelative("NumChunk"),new GUIContent("Num Chunk"));
            EditorGUILayout.IntSlider(mapSettings.FindPropertyRelative("PointPerMeter"), 2, 10);
            
            EditorGUILayout.Space(3);
            
            GUI.enabled = false;
            int chunkSz = mapSettings.FindPropertyRelative("ChunkSize").intValue;
            int nbChunk = mapSettings.FindPropertyRelative("NumChunk").intValue;
            int nbPoint = mapSettings.FindPropertyRelative("PointPerMeter").intValue;

            EditorGUILayout.IntField("Map Size", uiSettings.MapSettings.MapSize = chunkSz * nbChunk);
            EditorGUILayout.FloatField("Point Spacing", uiSettings.MapSettings.PointSpacing = 1f / (nbPoint - 1f));
            EditorGUILayout.IntField("ChunkPointPerAxis", uiSettings.MapSettings.ChunkPointPerAxis = (chunkSz * nbPoint) - (chunkSz - 1));
            EditorGUILayout.IntField("MapPointPerAxis", uiSettings.MapSettings.MapPointPerAxis = (nbChunk * chunkSz) * nbPoint - (nbChunk * chunkSz - 1));
            GUI.enabled = true;
            EditorGUILayout.Space(2);
        }

        void PoissonDiscGUI(UIMapSettings uiSettings)
        {
            GUI.enabled = false;
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(mapSettings, new GUIContent("Poisson Disc Settings"));
            EditorGUILayout.Space();
            GUI.enabled = true;
            EditorGUILayout.PropertyField(mapSettings.FindPropertyRelative("Seed"), new GUIContent("Seed"));
            EditorGUILayout.PropertyField(mapSettings.FindPropertyRelative("CellSize"), new GUIContent("CellSize"));
            
            EditorGUILayout.Space(3);
            
            GUI.enabled = false;
            int cellSz = mapSettings.FindPropertyRelative("CellSize").intValue;
            int chunkSz = mapSettings.FindPropertyRelative("ChunkSize").intValue;
            int nbChunk = mapSettings.FindPropertyRelative("NumChunk").intValue;

            EditorGUILayout.IntField("NumCellMap", uiSettings.MapSettings.NumCellMap = (int)ceil(chunkSz / (float)max(1, cellSz) * nbChunk));
            EditorGUILayout.FloatField("Radius", uiSettings.MapSettings.Radius = max(1f, cellSz) / SQRT2);
            GUI.enabled = true;
            EditorGUILayout.Space(2);
        }

        void PerlinNoiseGUI(UIMapSettings input)
        {
            showNoise.target = EditorGUILayout.Foldout(showNoise.target, noiseStatus, EditorStyles.foldoutHeader);
            if (EditorGUILayout.BeginFadeGroup(showNoise.faded))
            {
                EditorGUI.indentLevel++;
                
                int octaveVal = select(max(0, input.NoiseSettings.Octaves), 4, input.NoiseSettings.Octaves == 0);
                input.NoiseSettings.Octaves = EditorGUILayout.DelayedIntField("Octaves: ",  octaveVal);
                
                float lacunarityVal = select(max(0,min(2,input.NoiseSettings.Lacunarity)), 2f, input.NoiseSettings.Lacunarity == 0);
                input.NoiseSettings.Lacunarity = EditorGUILayout.DelayedFloatField("Lacunarity: ", lacunarityVal);
                
                input.NoiseSettings.Persistence = EditorGUILayout.Slider("Persistence: ",input.NoiseSettings.Persistence, 0, 1f);
                
                input.NoiseSettings.Scale = EditorGUILayout.FloatField("Scale: ", max(1f,input.NoiseSettings.Scale));
                input.NoiseSettings.HeightMultiplier = EditorGUILayout.FloatField("Height Multiplier: ", max(1f,input.NoiseSettings.HeightMultiplier));
                input.NoiseSettings.Offset = EditorGUILayout.Vector2Field("Offset: ", input.NoiseSettings.Offset);

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            UIMapSettings uiSettings = (UIMapSettings)target;
            InitPropreties();
            
            newGame.boolValue = EditorGUILayout.Toggle("New Game", newGame.boolValue);
            if (newGame.boolValue)
            {
                MapSettingGUI(uiSettings);
                PoissonDiscGUI(uiSettings);
                PerlinNoiseGUI(uiSettings);
            }
            else
            {
                directory = new DirectoryInfo(savesFolder);

                for (int i = 0; i < numSaves; i++)
                {
                    pos[i] = EditorGUILayout.Toggle(files[i], pos[i]);
                }
            }
            
            EditorGUILayout.Space();
            
            EditorGUI.BeginDisabledGroup(uiSettings.FolderName == string.Empty);
            if (GUILayout.Button("Generate"))
            {
                if (uiSettings.FolderName != string.Empty)
                {
                    //Debug.Log($"test bool bitwise {0 == 0 & 1 == 0}");
                    //Debug.Log($"{(int)(11 >> 1)}");
                    mapSystemPrefab = serializedObject.FindProperty("MapSystemPrefab");
                    AsyncOperationHandle<GameObject> csHandle = LoadSingleAssetSync<GameObject>(uiSettings.MapSystemPrefab);
                    csHandle.Result.GetComponent<MapSystem>().LoadMap(uiSettings.MapSettings, uiSettings.NoiseSettings, newGame.boolValue, uiSettings.FolderName);
                    uiSettings.ui.DebuggingEnable(uiSettings.MapSettings, uiSettings.FolderName);
                    uiSettings.ui.VoronoiEnabling();
                    Addressables.Release(csHandle);
                }
                else
                    Debug.Log("enter Folder Name");
                
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }

        //Found here : https://forum.unity.com/threads/find-all-files-in-folder-and-return-their-path-or-filename.941216/
        void GetFilesName(in string folderPath, ref string[] files, in int numSaves, in DirectoryInfo d)
        {
            DirectoryInfo[] filesInfo = directory.GetDirectories();
            for (int i = 0; i < numSaves; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(filesInfo[i].Name);
            }
        }

        void DirectoryAndNumSaves(out int numFiles, out DirectoryInfo dir)
        {
            dir = new DirectoryInfo(savesFolder);
            numFiles = directory.GetDirectories().Length;
        }

        private void InitPropreties()
        {
            mapSettings = serializedObject.FindProperty("MapSettings");
            newGame = serializedObject.FindProperty("NewGame");
            mapSystemPrefab = serializedObject.FindProperty("MapSystemPrefab");
            folderName = serializedObject.FindProperty("FolderName");
        }
    }
    #endif
}

