using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using RTS.Data;

public class QuickSetupBuildingActions
{
    [MenuItem("RTS/Quick Setup Building Actions")]
    public static void Setup()
    {
        // Create folder
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/BuildingActions"))
        {
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "BuildingActions");
        }

        // Create Sell action
        BuildingActionData sellAction = ScriptableObject.CreateInstance<BuildingActionData>();
        sellAction.actionId = "sell";
        sellAction.displayName = "Sell";
        sellAction.creditsCost = 0;
        sellAction.powerCost = 0;
        sellAction.hotkey = Key.S;
        AssetDatabase.CreateAsset(sellAction, "Assets/ScriptableObjects/BuildingActions/Action_Sell.asset");

        // Create Upgrade action
        BuildingActionData upgradeAction = ScriptableObject.CreateInstance<BuildingActionData>();
        upgradeAction.actionId = "upgrade";
        upgradeAction.displayName = "Upgrade";
        upgradeAction.creditsCost = 0;
        upgradeAction.powerCost = 0;
        upgradeAction.hotkey = Key.U;
        AssetDatabase.CreateAsset(upgradeAction, "Assets/ScriptableObjects/BuildingActions/Action_Upgrade.asset");

        // Create folder for configs
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/BuildingActionConfigs"))
        {
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "BuildingActionConfigs");
        }

        // Create Power Plant config
        BuildingActionConfig powerPlantConfig = ScriptableObject.CreateInstance<BuildingActionConfig>();
        powerPlantConfig.actions = new BuildingActionData[] { sellAction, upgradeAction };
        powerPlantConfig.selectionColor = new Color(0.2f, 0.8f, 1f);
        powerPlantConfig.outlineWidth = 3f;
        AssetDatabase.CreateAsset(powerPlantConfig, "Assets/ScriptableObjects/BuildingActionConfigs/Config_PowerPlant.asset");

        // Create Refinery config
        BuildingActionConfig refineryConfig = ScriptableObject.CreateInstance<BuildingActionConfig>();
        refineryConfig.actions = new BuildingActionData[] { sellAction, upgradeAction };
        refineryConfig.selectionColor = new Color(1f, 0.8f, 0.2f);
        refineryConfig.outlineWidth = 3f;
        AssetDatabase.CreateAsset(refineryConfig, "Assets/ScriptableObjects/BuildingActionConfigs/Config_Refinery.asset");

        // Create Barracks config
        BuildingActionConfig barracksConfig = ScriptableObject.CreateInstance<BuildingActionConfig>();
        barracksConfig.actions = new BuildingActionData[] { sellAction, upgradeAction };
        barracksConfig.selectionColor = new Color(0.2f, 1f, 0.2f);
        barracksConfig.outlineWidth = 3f;
        AssetDatabase.CreateAsset(barracksConfig, "Assets/ScriptableObjects/BuildingActionConfigs/Config_Barracks.asset");

        // Link to BuildingData assets
        BuildingData powerPlant = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_PowerPlant.asset");
        BuildingData refinery = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Refinery.asset");
        BuildingData barracks = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Barracks.asset");

        if (powerPlant != null)
        {
            powerPlant.actionConfig = powerPlantConfig;
            EditorUtility.SetDirty(powerPlant);
        }

        if (refinery != null)
        {
            refinery.actionConfig = refineryConfig;
            EditorUtility.SetDirty(refinery);
        }

        if (barracks != null)
        {
            barracks.actionConfig = barracksConfig;
            EditorUtility.SetDirty(barracks);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }
}
