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

namespace KaizerWaldCode
{
    [CustomEditor(typeof(UIMapSettings))]
    [CanEditMultipleObjects]
    public class EditorMapSettings : Editor
    {
        //private bool newGame;
        private SerializedProperty mapSettings;
        private SerializedProperty newGame;
        private SerializedProperty newGameName;
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
            mapSettings = serializedObject.FindProperty("mapSettings");
            newGame = serializedObject.FindProperty("NewGame");
            mapSystemPrefab = serializedObject.FindProperty("MapSystemPrefab");

            savesFolder = $"{Application.persistentDataPath}/Save Files/";
            if (!Directory.Exists(savesFolder))
            {
                Directory.CreateDirectory(savesFolder);
            }

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

            mapSettings = serializedObject.FindProperty("MapSettings");
            newGame = serializedObject.FindProperty("NewGame");
            newGameName = serializedObject.FindProperty("FolderName");

            newGame.boolValue = EditorGUILayout.Toggle("New Game", newGame.boolValue);
            if (newGame.boolValue)
            {
                //EditorGUILayout.TextField("Save Name", uiSettings.FolderName);
                EditorGUILayout.PropertyField(newGameName, new GUIContent("Save Name"));
                GUI.enabled = false;
                EditorGUILayout.PropertyField(mapSettings, new GUIContent("Map Settings"));
                GUI.enabled = true;
                EditorGUILayout.PropertyField(mapSettings.FindPropertyRelative("ChunkSize"), new GUIContent("Chunk Size"));
                EditorGUILayout.PropertyField(mapSettings.FindPropertyRelative("NumChunk"), new GUIContent("Num Chunk"));
                EditorGUILayout.IntSlider(mapSettings.FindPropertyRelative("PointPerMeter"), 2, 10);

                GUI.enabled = false;
                int chunkSz = mapSettings.FindPropertyRelative("ChunkSize").intValue;
                int nbChunk = mapSettings.FindPropertyRelative("NumChunk").intValue;
                int nbPoint = mapSettings.FindPropertyRelative("PointPerMeter").intValue;

                EditorGUILayout.IntField("Map Size", uiSettings.MapSettings.MapSize = chunkSz * nbChunk);
                EditorGUILayout.FloatField("Point Spacing", uiSettings.MapSettings.PointSpacing = 1f / (nbPoint - 1f));
                EditorGUILayout.IntField("ChunkPointPerAxis", uiSettings.MapSettings.ChunkPointPerAxis = (chunkSz * nbPoint) - (chunkSz - 1));
                EditorGUILayout.IntField("MapPointPerAxis", uiSettings.MapSettings.MapPointPerAxis = (nbChunk * chunkSz) * nbPoint - (nbChunk * chunkSz - 1));
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
                    csHandle.Result.GetComponent<MapSystem>().Initialize(uiSettings.MapSettings, uiSettings.FolderName);
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
    }
}
