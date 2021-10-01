using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWaldCode.Utils
{
    public static class NativeCollectionUtils
    {
        public static void AllocNtvArray(ref NativeArray<float3> array, int size)
        {
            array = new NativeArray<float3>(size, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }

        public static NativeArray<T> AllocNtvAry<T>(int size, NativeArrayOptions nativeArrayOptions = NativeArrayOptions.UninitializedMemory) where T : struct
        {
            return new NativeArray<T>(size, Allocator.TempJob, nativeArrayOptions);
        }

        public static NativeArray<T> AllocFillNtvAry<T>(int size, T val) where T : struct
        {
            NativeArray<T> a = new NativeArray<T>(size, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < size; i++) { a[i] = val; }
            return a;
        }

        public static NativeArray<T> ArrayToNativeArray<T>(T[] array,Allocator alloc = Allocator.TempJob ,NativeArrayOptions init = NativeArrayOptions.ClearMemory) where T : struct
        {
            NativeArray<T> nA = new NativeArray<T>(array.Length, alloc, init);
            nA.CopyFrom(array);
            return nA;
        }

        public static void Fill<T>(ref NativeArray<T> array, int arrayLength, T val) where T : struct
        {
            for (int i = 0; i < arrayLength; i++) { array[i] = val; }
        }
        
        public static int NumValueNotEqualTo<T>(in NativeArray<T> array, T val) where T : struct
        {
            int n = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(val)) continue;
                n++;
            }
            return n;
        }
        
        public static T[] RemoveDuplicates<T>(T[] s) where T : struct
        {
            HashSet<T> set = new HashSet<T>(s);
            T[] result = new T[set.Count];
            set.CopyTo(result);
            return result;
        }
        
        public static NativeArray<T> RemoveDuplicates<T>(in NativeArray<T> s, Allocator a = Allocator.TempJob, NativeArrayOptions nao = NativeArrayOptions.UninitializedMemory) 
            where T : struct
        {
            Debug.Log($"base NativeArray length = {s.Length}");
            HashSet<T> set = new HashSet<T>(s);
            NativeArray<T> result = new NativeArray<T>(set.Count, a, nao);
            Debug.Log($"NEW NativeArray length = {set.Count}");
            result.CopyFrom(s);
            return result;
        }
    }
}
