using System;
using System.Collections;
using System.Collections.Generic;
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
                byte* pDest = pInto;
                *((int*)pDest) = size;
                pDest += sizeof(int);
 
                // Name
                fixed (char* namePointer = saveGame.Name)
                {
                    char* namePtr = namePointer;
                    while (*namePtr != '\0')
                    {
                        *((char*)pDest) = *namePtr;
                        pDest += sizeof(char);
                        namePtr++;
                    }
                    *((char*)pDest) = '\0';
                    pDest += sizeof(char);
                }
 
                // HighScore
                *((int*)pDest) = saveGame.HighScore;
                pDest += sizeof(int);
            }
            return bytes;
        }
 
        public static bool Deserialize(byte[] bytes, ref SaveGame into)
        {
            fixed (byte* pBytes = bytes)
            {
                // Need at least enough for required size
                int size = bytes.Length;
                if (size < sizeof(int))
                {
                    return false;
                }
 
                byte* pSrc = pBytes;
 
                // Required size
                int requiredSize = *((int*)pSrc);
                if (size < requiredSize)
                {
                    return false;
                }
                pSrc += sizeof(int);
 
                // Name
                into.Name = new string((char*)pSrc);
                pSrc += 2 * into.Name.Length + sizeof(char);
 
                // HighScore
                into.HighScore = *((int*)pSrc);
                pSrc += sizeof(int);
            }
            return true;
        }
    }
}
