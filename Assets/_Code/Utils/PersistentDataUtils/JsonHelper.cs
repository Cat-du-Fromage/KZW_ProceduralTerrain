using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace KaizerWaldCode.Utils
{
    // See : https://stackoverflow.com/questions/36239705/serialize-and-deserialize-json-and-json-array-in-unity
    public static class JsonHelper
    {
        [Serializable]
        private class Wrapper<T> where T : struct
        {
            public T[] Items;
        }
        
        [Serializable]
        private class NativeWrapper<T> where T : struct
        {
            public NativeArray<T> Items;
        }
        
        public static T[] FromJson<T>(in string path) where T : struct
        {
            using (StreamReader stream = new StreamReader(path))
            {
                string json = stream.ReadToEnd();
                Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
                return wrapper.Items;
            } ;
        }
        
        public static void FromJson<T>(in string path, ref NativeArray<T> data) where T : struct
        {
            using (StreamReader stream = new StreamReader(path))
            {
                string json = stream.ReadToEnd();
                JsonUtility.FromJsonOverwrite(json, data);
            } ;
        }

        public static void ToJson<T>(T[] array, in string path, bool prettyPrint = false) where T : struct
        {
            using (StreamWriter stream = new StreamWriter(path))
            {
                Wrapper<T> wrapper = new Wrapper<T>();
                wrapper.Items = array;
                stream.Write(JsonUtility.ToJson(wrapper, prettyPrint));
            }
        }
        //Doesn't seems to works
        public static void ToJson<T>(NativeArray<T> array, in string path, bool prettyPrint = false) where T : struct
        {
            using (StreamWriter stream = new StreamWriter(path))
            {
                NativeWrapper<T> wrapper = new NativeWrapper<T>();
                wrapper.Items = array;
                stream.Write(JsonUtility.ToJson(wrapper, prettyPrint));
            }
        }
        
        //ASYNC METHOD
        public async static Task<T[]> FromJsonAsync<T>(string path) where T : struct
        {
            using (StreamReader stream = new StreamReader(path))
            {
                string json = await stream.ReadToEndAsync(); 
                Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
                return wrapper.Items;
            } ;
        }
    }
}
