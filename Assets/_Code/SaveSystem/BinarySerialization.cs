using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Mathematics;

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
            using (FileStream stream = File.Create(fullPath)) ;
        }

        public static T[] Load2<T>(in string fullPath, in int length) where T : struct
        {
            BinaryFormatter bf = new BinaryFormatter();

            T[] saveObject = new T[length];
            if (SaveExist(fullPath))
            {
                using (FileStream file = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {
                    if (file.Length != 0)
                    {
                        saveObject = (T[])bf.Deserialize(file);
                    }
                }
            }
            return saveObject;
        }

        /*
        public static float3[] LoadFloat3(in string fullPath)
        {
            FileStream stream = new FileStream(fullPath, FileMode.Open);

            long numValues = stream.Length / (3 * sizeof(float));
            float3[] readValues = new float3[numValues];

            float3[] saveObject = new float3[] { };

            while (bufferedReader.FillBuffer())
            {
                // Read as many values as we can from the reader's buffer
                var readValsIndex = 0;
                for (
                    var numReads = bufferedReader.NumBytesAvailable / sizeof(ushort);
                    numReads > 0;
                    --numReads
                )
                {
                    readValues[readValsIndex++] = bufferedReader.ReadUInt16();
                }
            }



            return saveObject;
        }

        public static float3 ReadFloat3()
        {
            var val = (ushort)((int)buffer[bufferOffset] | (int)buffer[bufferOffset + 1] << 8);
            bufferOffset += 2;
            return val;
        }
        */
    }
}
