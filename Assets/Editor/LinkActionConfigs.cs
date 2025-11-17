using UnityEngine;
using UnityEditor;
using RTS.Data;

public class LinkActionConfigs
{
    [MenuItem("RTS/Link Action Configs NOW")]
    public static void LinkNow()
    {
        Debug.Log("Linking action configs...");

        // Load configs
        BuildingActionConfig powerPlantConfig = AssetDatabase.LoadAssetAtPath<BuildingActionConfig>("Assets/ScriptableObjects/BuildingActionConfigs/Config_PowerPlant.asset");
        BuildingActionConfig refineryConfig = AssetDatabase.LoadAssetAtPath<BuildingActionConfig>("Assets/ScriptableObjects/BuildingActionConfigs/Config_Refinery.asset");
        BuildingActionConfig barracksConfig = AssetDatabase.LoadAssetAtPath<BuildingActionConfig>("Assets/ScriptableObjects/BuildingActionConfigs/Config_Barracks.asset");

        // Load BuildingData assets
        BuildingData powerPlant = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_PowerPlant.asset");
        BuildingData refinery = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Refinery.asset");
        BuildingData barracks = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Barracks.asset");

        // Link them
        if (powerPlant != null && powerPlantConfig != null)
        {
            powerPlant.actionConfig = powerPlantConfig;
            EditorUtility.SetDirty(powerPlant);
            Debug.Log("✅ Linked PowerPlant");
        }

        if (refinery != null && refineryConfig != null)
        {
            refinery.actionConfig = refineryConfig;
            EditorUtility.SetDirty(refinery);
            Debug.Log("✅ Linked Refinery");
        }

        if (barracks != null && barracksConfig != null)
        {
            barracks.actionConfig = barracksConfig;
            EditorUtility.SetDirty(barracks);
            Debug.Log("✅ Linked Barracks");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("✅ ALL LINKED! Now enter Play Mode and test!");
    }
}
