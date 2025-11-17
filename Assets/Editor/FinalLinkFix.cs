using UnityEngine;
using UnityEditor;
using RTS.Data;

public static class FinalLinkFix
{
    [MenuItem("RTS/FINAL FIX - Link Configs")]
    public static void Fix()
    {
        LinkConfigs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("=== DONE ===");
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            // Check if needed
            var barracks = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Barracks.asset");
            if (barracks != null && barracks.actionConfig == null)
            {
                LinkConfigs();
            }
        };
    }

    static void LinkConfigs()
    {
        Debug.Log("Linking configs...");

        var configs = new (string dataPath, string configPath)[]
        {
            ("Assets/BuildingData/BuildingData_PowerPlant.asset", "Assets/ScriptableObjects/BuildingActionConfigs/Config_PowerPlant.asset"),
            ("Assets/BuildingData/BuildingData_Refinery.asset", "Assets/ScriptableObjects/BuildingActionConfigs/Config_Refinery.asset"),
            ("Assets/BuildingData/BuildingData_Barracks.asset", "Assets/ScriptableObjects/BuildingActionConfigs/Config_Barracks.asset")
        };

        foreach (var (dataPath, configPath) in configs)
        {
            var data = AssetDatabase.LoadAssetAtPath<BuildingData>(dataPath);
            var config = AssetDatabase.LoadAssetAtPath<BuildingActionConfig>(configPath);

            if (data != null && config != null)
            {
                data.actionConfig = config;
                EditorUtility.SetDirty(data);
                Debug.Log($"✅ Linked {System.IO.Path.GetFileNameWithoutExtension(dataPath)}");
            }
        }

        AssetDatabase.SaveAssets();
    }
}
