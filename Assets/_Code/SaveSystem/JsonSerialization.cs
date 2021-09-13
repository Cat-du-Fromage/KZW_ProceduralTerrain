using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
//using static UnityEditor.EditorJsonUtility;

namespace KaizerWaldCode.KWSerialization
{
    public static class JsonSerialization
    {
        public static void Save<T>(in string fullPath, T data) where T : struct
        {
            if (!Directory.Exists(fullPath)) 
                Directory.CreateDirectory(fullPath);

            string json = JsonUtility.ToJson(data);
            File.WriteAllText($"{fullPath}.json", json);
        }

        public static void SaveArray<T>(in string fullPath, in string fileName , T[] data) where T : struct
        {
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            File.Create($"{fullPath}/{fileName}.json");

            string json = JsonUtility.ToJson(data);
            File.WriteAllText($"{fullPath}/{fileName}.json", json);
            /*
            using (StreamWriter sw = new StreamWriter(Path.Combine(fullPath, $"{fileName}.json")))
            {
                sw.Write(json);
            }
            */
            //File.WriteAllText($"{fullPath}.json", json);
        }

        public static T Load<T>(in string fullPath) where T : struct
        {
            T saveObject = new T();
            string jsonFile = File.ReadAllText($"{fullPath}.json");
            saveObject = JsonUtility.FromJson<T>(jsonFile);
            return saveObject;
        }

        public static T[] LoadArray<T>(in string fullPath) where T : struct
        {
            T[] saveObject = new T[]{};
            string jsonFile = File.ReadAllText($"{fullPath}.json");
            saveObject = JsonUtility.FromJson<T[]>(jsonFile);
            return saveObject;
        }

        public static bool SaveExist(in string fullPath)
        {
            return File.Exists($"{fullPath}.json");
        }
    }
}
