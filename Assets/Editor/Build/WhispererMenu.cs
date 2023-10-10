using UnityEditor;
using UnityEngine;

namespace Editor.Build
{
    public static class WhispererMenu
    {
        [MenuItem("Whisperer/Clear Player Data")]
        public static void ClearData()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
