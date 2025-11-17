using UnityEngine;
using UnityEditor;
using RTS.Data;

public class LinkBuildingDataAssets
{
    [MenuItem("RTS/Link Building Data to Prefabs")]
    public static void LinkBuildingData()
    {
        // Load BuildingData assets
        BuildingData powerPlant = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_PowerPlant.asset");
        BuildingData refinery = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Refinery.asset");
        BuildingData barracks = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Barracks.asset");

        // Load prefabs
        GameObject powerPlantPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BuildingPrefab_PowerPlant.prefab");
        GameObject refineryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BuildingPrefab_Refinery.prefab");
        GameObject barracksPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BuildingPrefab_Barracks.prefab");

        // Link prefabs to BuildingData
        if (powerPlant != null && powerPlantPrefab != null)
        {
            powerPlant.prefab = powerPlantPrefab;
            EditorUtility.SetDirty(powerPlant);
        }

        if (refinery != null && refineryPrefab != null)
        {
            refinery.prefab = refineryPrefab;
            EditorUtility.SetDirty(refinery);
        }

        if (barracks != null && barracksPrefab != null)
        {
            barracks.prefab = barracksPrefab;
            EditorUtility.SetDirty(barracks);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }
}
