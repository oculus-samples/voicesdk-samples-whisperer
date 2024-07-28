using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public sealed class ONSPSettings : ScriptableObject
{
    private static ONSPSettings instance;

    [SerializeField] public int voiceLimit = 64;

    public static ONSPSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<ONSPSettings>("ONSPSettings");

                // This can happen if the developer never input their App Id into the Unity Editor
                // and therefore never created the OculusPlatformSettings.asset file
                // Use a dummy object with defaults for the getters so we don't have a null pointer exception
                if (instance == null)
                {
                    instance = CreateInstance<ONSPSettings>();

#if UNITY_EDITOR
                    // Only in the editor should we save it to disk
                    var properPath = Path.Combine(Application.dataPath, "Resources");
                    if (!Directory.Exists(properPath)) AssetDatabase.CreateFolder("Assets", "Resources");

                    var fullPath = Path.Combine(
                        Path.Combine("Assets", "Resources"),
                        "ONSPSettings.asset");
                    AssetDatabase.CreateAsset(instance, fullPath);
#endif
                }
            }

            return instance;
        }
    }
}
