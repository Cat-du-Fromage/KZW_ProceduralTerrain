using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace KaizerWaldCode.KWSerialization
{
    public static class BinarySerialization
    {
        public static void Save<T>(in string fullPath, T[] data) where T : struct
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file;
            if (!SaveExist(fullPath))
            {
                file = File.Create($"{fullPath}.dat");
                bf.Serialize(file, data);
                file.Close();
            }
            else
            {
                file = File.Open($"{fullPath}.dat", FileMode.Open, FileAccess.Write);
                bf.Serialize(file, data);
                file.Close();
            }
        }

        public static T[] Load<T>(in string fullPath) where T : struct
        {
            BinaryFormatter bf = new BinaryFormatter();

            T[] saveObject = new T[] { };
            if (!SaveExist(fullPath))
            {
                FileStream file = File.Open($"{fullPath}.dat", FileMode.Open, FileAccess.Read);
                saveObject = (T[])bf.Deserialize(file);
                file.Close();
            }

            return saveObject;
        }

        public static bool SaveExist(in string fullPath)
        {
            return File.Exists($"{fullPath}.dat");
        }
    }
}
