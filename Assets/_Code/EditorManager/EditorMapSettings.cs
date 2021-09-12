using System.Collections;
using System.Collections.Generic;
using System.IO;
using KaizerWaldCode.TerrainGeneration;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.TerrainGeneration.KwEntity;
using KaizerWaldCode.TerrainGeneration.KwSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using static KaizerWaldCode.Utils.AddressablesUtils;
using static Unity.Mathematics.math;

#if UNITY_EDITOR
namespace KaizerWaldCode.KwEditor
{
    [CustomEditor(typeof(UIMapSettings))]
    [CanEditMultipleObjects]
    public class EditorMapSettings : Editor
    {
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

        void Awake()
        {
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

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            UIMapSettings uiSettings = (UIMapSettings)target;
            InitPropreties();

            newGame.boolValue = EditorGUILayout.Toggle("New Game", newGame.boolValue);
            if (newGame.boolValue)
            {
                //EditorGUILayout.TextField("Save Name", uiSettings.FolderName);
                EditorGUILayout.PropertyField(folderName, new GUIContent("Save Name"));
                GUI.enabled = false;
                EditorGUILayout.PropertyField(mapSettings, new GUIContent("Map Settings"));
                GUI.enabled = true;
                EditorGUILayout.PropertyField(mapSettings.FindPropertyRelative("ChunkSize"), new GUIContent("Chunk Size"));
                EditorGUILayout.PropertyField(mapSettings.FindPropertyRelative("NumChunk"), new GUIContent("Num Chunk"));
                EditorGUILayout.IntSlider(mapSettings.FindPropertyRelative("PointPerMeter"), 2, 10);
                EditorGUILayout.PropertyField(mapSettings.FindPropertyRelative("Seed"), new GUIContent("Seed"));
                EditorGUILayout.PropertyField(mapSettings.FindPropertyRelative("Radius"), new GUIContent("Radius"));

                GUI.enabled = false;
                int chunkSz = mapSettings.FindPropertyRelative("ChunkSize").intValue;
                int nbChunk = mapSettings.FindPropertyRelative("NumChunk").intValue;
                int nbPoint = mapSettings.FindPropertyRelative("PointPerMeter").intValue;
                int radius = mapSettings.FindPropertyRelative("Radius").intValue;

                EditorGUILayout.IntField("Map Size", uiSettings.MapSettings.MapSize = chunkSz * nbChunk);
                EditorGUILayout.FloatField("Point Spacing", uiSettings.MapSettings.PointSpacing = 1f / (nbPoint - 1f));
                EditorGUILayout.IntField("ChunkPointPerAxis", uiSettings.MapSettings.ChunkPointPerAxis = (chunkSz * nbPoint) - (chunkSz - 1));
                EditorGUILayout.IntField("MapPointPerAxis", uiSettings.MapSettings.MapPointPerAxis = (nbChunk * chunkSz) * nbPoint - (nbChunk * chunkSz - 1));
                EditorGUILayout.IntField("NumCellMap", uiSettings.MapSettings.NumCellMap = (int)ceil(chunkSz / (float)max(1, radius) * nbChunk));
                EditorGUILayout.FloatField("CellSize", uiSettings.MapSettings.CellSize = max(1f, radius) / SQRT2);
                GUI.enabled = true;
            }
            else
            {
                directory = new DirectoryInfo(savesFolder);

                for (int i = 0; i < numSaves; i++)
                {
                    pos[i] = EditorGUILayout.Toggle(files[i], pos[i]);
                }
            }

            if (GUILayout.Button("Generate"))
            {
                if (uiSettings.FolderName != string.Empty)
                {
                    mapSystemPrefab = serializedObject.FindProperty("MapSystemPrefab");
                    AsyncOperationHandle<GameObject> csHandle = LoadSingleAssetSync<GameObject>(uiSettings.MapSystemPrefab);
                    csHandle.Result.GetComponent<MapSystem>().LoadMap(uiSettings.MapSettings, newGame.boolValue, uiSettings.FolderName);
                    Addressables.Release(csHandle);
                }
                else
                    Debug.Log("enter Folder Name");
            }


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
}
#endif