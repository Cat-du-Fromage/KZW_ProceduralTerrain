using System.Collections;
using System.Collections.Generic;
using System.IO;
using KaizerWaldCode.TerrainGeneration;
using KaizerWaldCode.TerrainGeneration.Data;
using KaizerWaldCode.TerrainGeneration.KwEntity;
using UnityEditor;
using UnityEngine;

namespace KaizerWaldCode
{
    [CustomEditor(typeof(UIMapSettings))]
    [CanEditMultipleObjects]
    public class EditorMapSettings : Editor
    {
        [Header("New Game")]
        private bool newGame;
        private bool stateChange_NewGame;
        private SerializedProperty mapSettings;

        private bool posGroupEnabled = true;

        //Save Files
        [Header("Save Files")] 
        public string[] files;
        bool[] pos = new bool[3] { true, true, true };
        
        //Internal Private fields
        private string savesFolder;
        private DirectoryInfo directory;
        private int numSaves;

        void Awake()
        {
            //newGame = serializedObject.FindProperty("newGame");
            mapSettings = serializedObject.FindProperty("mapSettings");

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

            if (numSaves != directory.GetFiles("*.dat").Length)
            {
                numSaves = directory.GetFiles("*.dat").Length;
                files = new string[numSaves];
            }

            GetFilesName(savesFolder, ref files, numSaves, directory);
        }

        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();
            UIMapSettings uiSettings = (UIMapSettings)target;

            mapSettings = serializedObject.FindProperty("mapSettings");
            uiSettings.newGame = EditorGUILayout.Toggle("New Game", uiSettings.newGame);
            if (uiSettings.newGame)
            {
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

                EditorGUILayout.IntField("Map Size", chunkSz * nbChunk);
                EditorGUILayout.FloatField("Point Spacing", 1f / (nbPoint - 1f));
                EditorGUILayout.IntField("ChunkPointPerAxis", (chunkSz * nbPoint) - (chunkSz - 1));
                EditorGUILayout.IntField("MapPointPerAxis", (nbChunk * chunkSz) * nbPoint - (nbChunk * chunkSz - 1));
                GUI.enabled = true;
                //EditorGUILayout.IntField("Chunk Size", uiSettings.mapSettings.ChunkSize);
            }
            else
            {
                directory = new DirectoryInfo(savesFolder);
                using (var posGroup = new EditorGUILayout.ToggleGroupScope("Align position", posGroupEnabled))
                {
                    for (int i = 0; i < numSaves; i++)
                    {
                        posGroupEnabled = posGroup.enabled;
                        pos[i] = EditorGUILayout.Toggle(files[i], pos[i]);
                    }
                }

                if (GUILayout.Button("Generate"))
                {
                    Debug.Log(mapSettings.FindPropertyRelative("ChunkSize").intValue);
                }
            }

            

            serializedObject.ApplyModifiedProperties();
        }

        //Found here : https://forum.unity.com/threads/find-all-files-in-folder-and-return-their-path-or-filename.941216/
        void GetFilesName(in string folderPath, ref string[] files, in int numSaves, in DirectoryInfo d)
        {
            FileInfo[] filesInfo = d.GetFiles("*.dat");
            for (int i = 0; i < numSaves; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(filesInfo[i].Name);
                Debug.Log(files[i]);
            }
        }

        void DirectoryAndNumSaves(out int numFiles, out DirectoryInfo dir)
        {
            dir = new DirectoryInfo(savesFolder);
            numFiles = directory.GetFiles("*.dat").Length;
        }
    }
}
