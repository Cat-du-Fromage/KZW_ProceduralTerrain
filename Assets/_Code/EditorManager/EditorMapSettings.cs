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
    [CustomEditor(typeof(ChunkSliceSystem))]
    [CanEditMultipleObjects]
    public class EditorMapSettings : Editor
    {
        [Header("New Game")]
        [SerializeField] private SerializedProperty newGame;
        [ConditionalHide(nameof(newGame), true)]
        [SerializeField] private bool posGroupEnabled = true;

        [Header("Save Files")] 
        public string[] files;
        bool[] pos = new bool[3] { true, true, true };
        

        private string savesFolder;
        private DirectoryInfo directory;
        private int numSaves;

        void Awake()
        {
            //newGame = serializedObject.FindProperty("newGame"); //Need to be attached to the MonoScript

            savesFolder = $"{Application.persistentDataPath}/Save Files/";
            if (!Directory.Exists(savesFolder))
            {
                Directory.CreateDirectory(savesFolder);
            }

            DirectoryAndNumSaves(out numSaves, out directory);
            //directory = new DirectoryInfo(savesFolder);
            //numSaves = directory.GetFiles("*.dat").Length;
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
            DrawDefaultInspector();

            //newGame.boolValue = EditorGUILayout.Toggle("A Boolean", newGame.boolValue);
            

            directory = new DirectoryInfo(savesFolder);
            /*
            if (numSaves != directory.GetFiles("*.dat").Length)
            {
                numSaves = directory.GetFiles("*.dat").Length;
                files = new string[numSaves];
                FilesSavedGames(savesFolder, ref files, numSaves, directory);
            }
            */
            using (var posGroup = new EditorGUILayout.ToggleGroupScope("Align position", posGroupEnabled))
            {
                for (int i = 0; i < numSaves; i++)
                {
                    posGroupEnabled = posGroup.enabled;
                    pos[i] = EditorGUILayout.Toggle(files[i], pos[i]);
                }
            }
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
