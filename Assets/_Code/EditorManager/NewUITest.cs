using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KaizerWaldCode
{
    public class NewUITest : MonoBehaviour
    {
        public bool test;
    }
#if UNITY_EDITOR  
    [CustomEditor(typeof(NewUITest))]
    public class NewUITestEditor : Editor
    {
        
    }
#endif
}
