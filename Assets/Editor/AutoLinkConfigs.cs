using UnityEngine;
using UnityEditor;
using RTS.Data;

[InitializeOnLoad]
public class AutoLinkConfigs
{
    static AutoLinkConfigs()
    {
        EditorApplication.delayCall += LinkConfigs;
    }

    static void LinkConfigs()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        bool needsLink = false;

        // Check if linking is needed
        BuildingData barracks = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Barracks.asset");
        if (barracks != null && barracks.actionConfig == null)
        {
            needsLink = true;
        }

        if (!needsLink) return;

        Debug.Log("[AutoLink] Linking action configs...");

        // Load configs
        BuildingActionConfig powerPlantConfig = AssetDatabase.LoadAssetAtPath<BuildingActionConfig>("Assets/ScriptableObjects/BuildingActionConfigs/Config_PowerPlant.asset");
        BuildingActionConfig refineryConfig = AssetDatabase.LoadAssetAtPath<BuildingActionConfig>("Assets/ScriptableObjects/BuildingActionConfigs/Config_Refinery.asset");
        BuildingActionConfig barracksConfig = AssetDatabase.LoadAssetAtPath<BuildingActionConfig>("Assets/ScriptableObjects/BuildingActionConfigs/Config_Barracks.asset");

        // Load and link BuildingData
        BuildingData powerPlant = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_PowerPlant.asset");
        if (powerPlant != null && powerPlantConfig != null)
        {
            powerPlant.actionConfig = powerPlantConfig;
            EditorUtility.SetDirty(powerPlant);
            Debug.Log("[AutoLink] ✅ Linked PowerPlant");
        }

        BuildingData refinery = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Refinery.asset");
        if (refinery != null && refineryConfig != null)
        {
            refinery.actionConfig = refineryConfig;
            EditorUtility.SetDirty(refinery);
            Debug.Log("[AutoLink] ✅ Linked Refinery");
        }

        if (barracks != null && barracksConfig != null)
        {
            barracks.actionConfig = barracksConfig;
            EditorUtility.SetDirty(barracks);
            Debug.Log("[AutoLink] ✅ Linked Barracks");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[AutoLink] ✅ ALL DONE! Enter Play Mode and select a building - you should see buttons now!");
    }
}
