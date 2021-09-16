using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KaizerWaldCode
{
    [Serializable]
    public struct SaveGame
    {
        public string Name;
        public int HighScore;
    }
    
    public unsafe static class BytesSerialization
    {
        public static byte[] BytesSerialize(ref SaveGame saveGame)
        {
            // Required size + HighScore
            int size = 8 + saveGame.Name.Length * 2 + sizeof(char); // Name
            byte[] bytes = new byte[size];
            fixed (byte* pInto = bytes)
            {
                // Required size
                byte* pDest = pInto; //when value of pDest is changed value pointed(bytes) is changed too
                *((int*)pDest) = size; // cast Byte:pDest into Int* (taking 4 memory space in the array (4 bytes))
                pDest += sizeof(int); // move the memory location by 4 byte(size of int) for further write
 
                // Name
                fixed (char* namePointer = saveGame.Name) //namePointer get the value of the first char of saveGame.Name (saveGame.Name[0])
                {
                    char* namePtr = namePointer; // use another namePtr so we can navigate (fixed prevent any move in the memory)
                    
                    while (*namePtr != '\0')
                    {
                        *((char*)pDest) = *namePtr; // give to *pDest(pointer value) the value of *namePtr(pointer value) CAREFULL it takes 2 bytes
                        pDest += sizeof(char); // pDest to +2 adresse memory (sizeof(char)) (OR we can see it as moving 2 byte in the array)
                        namePtr++; // move namePtr adresse memory of 1 (next char)
                    }
                    *((char*)pDest) = '\0'; //upon reaching the end of the string we give the "terminator char" value to pDest
                    pDest += sizeof(char); //move in memory of 2 bytes (needed because "\0" is ONE CHAR! so we need 2 more bytes
                }
 
                // HighScore
                *((int*)pDest) = saveGame.HighScore;
                pDest += sizeof(int);
            }
            return bytes;
        }
 
        public static bool BytesDeserialize(byte[] bytes, ref SaveGame into)
        {
            fixed (byte* pBytes = bytes)
            {
                // Need at least enough for required size
                int size = bytes.Length;
                Debug.Log($"BytesDeserialize int size(bytes.Length) == {bytes.Length}");
                if (size < sizeof(int))
                {
                    return false;
                }
 
                byte* pSrc = pBytes;
                Debug.Log($"BytesDeserialize byte* pSrc(pBytes) == {pBytes->ToString()}");
                // Required size
                int requiredSize = *((int*)pSrc);
                Debug.Log($"BytesDeserialize requiredSize(*((int*)pSrc)) == {*((int*)pSrc)}");
                if (size < requiredSize)
                {
                    return false;
                }
                pSrc += sizeof(int);
                Debug.Log($"BytesDeserialize pSrc += sizeof(int); == {pSrc->ToString()}");
                // Name
                into.Name = new string((char*)pSrc);
                Debug.Log($"BytesDeserialize into.Name(new string((char*)pSrc)) == {new string((char*)pSrc)}");
                pSrc += 2 * into.Name.Length + sizeof(char);
                Debug.Log($"BytesDeserialize pSrc += 2 * into.Name.Length + sizeof(char); == {pSrc->ToString()}");
                // HighScore
                into.HighScore = *((int*)pSrc);
                Debug.Log($"BytesDeserialize into.HighScore = *((int*)pSrc); == {*((int*)pSrc)}");
                pSrc += sizeof(int);
            }
            return true;
        }
        
        //Try with for loop
        public static byte[] BytesSerialize2(ref SaveGame saveGame)
        {
            // Required size + HighScore
            int size = 8 + saveGame.Name.Length * 2 + sizeof(char); // Name
            byte[] bytes = new byte[size];
            fixed (byte* pInto = bytes)
            {
                // Required size
                byte* pDest = pInto; //when value of pDest is changed value pointed(bytes) is changed too
                *((int*)pDest) = size; // cast Byte:pDest into Int* (taking 4 memory space in the array (4 bytes))
                pDest += sizeof(int); // move the memory location by 4 byte(size of int) for further write
 
                // Name
                fixed (char* namePointer = saveGame.Name) //namePointer get the value of the first char of saveGame.Name (saveGame.Name[0])
                {
                    char* namePtr = namePointer; // use another namePtr so we can navigate (fixed prevent any move in the memory)

                    for (int i = 0; i < saveGame.Name.Length; i++)
                    {
                        *((char*)pDest) = *namePtr; // give to *pDest(pointer value) the value of *namePtr(pointer value) CAREFULL it takes 2 bytes
                        pDest += sizeof(char); // pDest to +2 adresse memory (sizeof(char)) (OR we can see it as moving 2 byte in the array)
                        namePtr++; // move namePtr adresse memory of 1 (next char)
                    }
                    *((char*)pDest) = '\0'; //upon reaching the end of the string we give the "terminator char" value to pDest
                    pDest += sizeof(char); //move in memory of 2 bytes (needed because "\0" is ONE CHAR! so we need 2 more bytes
                }
 
                // HighScore
                *((int*)pDest) = saveGame.HighScore;
                pDest += sizeof(int);
            }
            return bytes;
        }
        
        static void test()
        {
            SaveGame sv = new SaveGame
            {
                Name = "test",
                HighScore = 10,
            };
            Byte[] testBytes = BytesSerialization.BytesSerialize(ref sv);
            Debug.Log($"Bytes {testBytes.ToString()}");
            char[] tc = Encoding.UTF8.GetChars(testBytes);
            string s = Encoding.UTF8.GetString(testBytes);
            Encoding.UTF8.GetString(testBytes);
            Debug.Log($"Bytes string {s.Length}");
            
            for (int i = 0; i < tc.Length; i++)
            {
                Debug.Log($"Bytes at {i} = {(int)tc[i]}");
            }

            SaveGame svDeser = new SaveGame();

            bool succed = BytesSerialization.BytesDeserialize(testBytes, ref svDeser);
            Debug.Log($"succed? = {succed}");
            Debug.Log($"SaveGame = {svDeser.Name} + {svDeser.HighScore}");
            
        }
    }
}
