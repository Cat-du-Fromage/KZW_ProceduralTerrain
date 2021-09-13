using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ZeroFormatter;
using ZeroFormatter.Formatters;

namespace KaizerWaldCode.KWSerialization
{
    public static class BinarySerialization
    {
        public static void Save<T>(in string fullPath, in T[] data) where T : struct
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file;
            if (!SaveExist(fullPath))
            {
                file = File.Create(fullPath);
                bf.Serialize(file, data);
            }
            else
            {
                File.WriteAllText(fullPath, string.Empty);
                file = File.Open(fullPath, FileMode.Open, FileAccess.Write);
                bf.Serialize(file, data);
            }
            file.Close();
            file.Dispose();
        }

        public static T[] Load<T>(in string fullPath) where T : struct
        {
            BinaryFormatter bf = new BinaryFormatter();

            T[] saveObject = new T[] { };
            if (SaveExist(fullPath))
            {
                FileStream file = File.Open(fullPath, FileMode.Open, FileAccess.Read);
                if(file.Length != 0)
                {
                    saveObject = bf.Deserialize(file) as T[];
                    file.Close();
                    file.Dispose();
                }
            }
            return saveObject;
        }

        public static bool SaveExist(in string fullPath)
        {
            return File.Exists(fullPath);
        }

        public static void CreateFile(in string fullPath)
        {
            FileStream stream = File.Create(fullPath);
            stream.Close();
            stream.Dispose();
        }

        public static void ZFSave<T>(in string fullPath, in T[] data) where T : struct
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file;
            if (!SaveExist(fullPath))
            {
                file = File.Create(fullPath);
                ZeroFormatterSerializer.Serialize(file, data);
                //bf.Serialize(file, data);
            }
            else
            {
                File.WriteAllText(fullPath, string.Empty);
                file = File.Open(fullPath, FileMode.Open, FileAccess.Write);
                ZeroFormatterSerializer.Serialize(file, data);
                //bf.Serialize(file, data);
            }
            file.Close();
            file.Dispose();
        }

        public static T[] ZFLoad<T>(in string fullPath) where T : struct
        {
            BinaryFormatter bf = new BinaryFormatter();

            T[] saveObject = new T[] { };
            if (SaveExist(fullPath))
            {
                FileStream file = File.Open(fullPath, FileMode.Open, FileAccess.Read);
                if (file.Length != 0)
                {
                    //saveObject = bf.Deserialize(file) as T[];
                    byte[] bytesRead = File.ReadAllBytes(fullPath);
                    saveObject = ZeroFormatterSerializer.Deserialize<T[]>(file);
                    file.Close();
                    file.Dispose();
                }
            }
            return saveObject;
        }
    }
}
