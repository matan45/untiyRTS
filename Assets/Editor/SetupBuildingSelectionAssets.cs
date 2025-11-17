using UnityEngine;
using UnityEditor;
using System.IO;
using RTS.Data;
using RTS.Selection;

/// <summary>
/// Automatically creates BuildingActionData and BuildingActionConfig assets for the building selection system.
/// </summary>
public class SetupBuildingSelectionAssets
{
    [MenuItem("RTS/Setup Building Selection Assets")]
    public static void CreateSelectionAssets()
    {
        string actionDataFolder = "Assets/ScriptableObjects/BuildingActions";
        string actionConfigFolder = "Assets/ScriptableObjects/BuildingActionConfigs";

        // Create folders if they don't exist
        CreateFolderIfNeeded("Assets/ScriptableObjects");
        CreateFolderIfNeeded(actionDataFolder);
        CreateFolderIfNeeded(actionConfigFolder);

        // Create common action data assets
        BuildingActionData sellAction = CreateSellAction(actionDataFolder);
        BuildingActionData upgradeAction = CreateUpgradeAction(actionDataFolder);
        BuildingActionData produceUnitAction = CreateProduceUnitAction(actionDataFolder);

        Debug.Log("✅ Created BuildingActionData assets");

        // Create action configs for each building type
        CreatePowerPlantConfig(actionConfigFolder, sellAction, upgradeAction);
        CreateRefineryConfig(actionConfigFolder, sellAction, upgradeAction);
        CreateBarracksConfig(actionConfigFolder, sellAction, upgradeAction, produceUnitAction);

        Debug.Log("✅ Created BuildingActionConfig assets");

        // Link configs to BuildingData assets
        LinkActionConfigsToBuildingData();

        // Update building prefabs with SelectionVisualController
        UpdateBuildingPrefabs();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ Building Selection Assets setup complete!");
        Debug.Log("📋 Next steps:\n1. Assign InputActionAsset to BuildingSelectionManager\n2. Add BuildingActionExecutor to GameManagers\n3. Create UI panel (see BUILDING_SELECTION_SETUP.md)");
    }

    private static void CreateFolderIfNeeded(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parentFolder = Path.GetDirectoryName(path).Replace("\\", "/");
            string folderName = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }
    }

    private static BuildingActionData CreateSellAction(string folder)
    {
        BuildingActionData action = ScriptableObject.CreateInstance<BuildingActionData>();
        action.actionId = "sell";
        action.displayName = "Sell Building";
        action.tooltip = "Sell this building for 50% refund";
        action.creditsCost = 0;
        action.powerCost = 0;
        action.hotkey = KeyCode.S;

        string path = folder + "/Action_Sell.asset";
        AssetDatabase.CreateAsset(action, path);
        return action;
    }

    private static BuildingActionData CreateUpgradeAction(string folder)
    {
        BuildingActionData action = ScriptableObject.CreateInstance<BuildingActionData>();
        action.actionId = "upgrade";
        action.displayName = "Upgrade Building";
        action.tooltip = "Upgrade this building to the next tier";
        action.creditsCost = 0; // Will be set by upgrade data
        action.powerCost = 0;
        action.hotkey = KeyCode.U;

        string path = folder + "/Action_Upgrade.asset";
        AssetDatabase.CreateAsset(action, path);
        return action;
    }

    private static BuildingActionData CreateProduceUnitAction(string folder)
    {
        BuildingActionData action = ScriptableObject.CreateInstance<BuildingActionData>();
        action.actionId = "produce_unit";
        action.displayName = "Train Unit";
        action.tooltip = "Train a unit from this building";
        action.creditsCost = 0; // Will be set by unit data
        action.powerCost = 0;
        action.hotkey = KeyCode.T;

        string path = folder + "/Action_ProduceUnit.asset";
        AssetDatabase.CreateAsset(action, path);
        return action;
    }

    private static void CreatePowerPlantConfig(string folder, BuildingActionData sellAction, BuildingActionData upgradeAction)
    {
        BuildingActionConfig config = ScriptableObject.CreateInstance<BuildingActionConfig>();
        config.actions = new BuildingActionData[] { sellAction, upgradeAction };
        config.selectionColor = new Color(0.2f, 0.8f, 1f); // Cyan
        config.outlineWidth = 3f;

        string path = folder + "/Config_PowerPlant.asset";
        AssetDatabase.CreateAsset(config, path);
    }

    private static void CreateRefineryConfig(string folder, BuildingActionData sellAction, BuildingActionData upgradeAction)
    {
        BuildingActionConfig config = ScriptableObject.CreateInstance<BuildingActionConfig>();
        config.actions = new BuildingActionData[] { sellAction, upgradeAction };
        config.selectionColor = new Color(1f, 0.8f, 0.2f); // Gold
        config.outlineWidth = 3f;

        string path = folder + "/Config_Refinery.asset";
        AssetDatabase.CreateAsset(config, path);
    }

    private static void CreateBarracksConfig(string folder, BuildingActionData sellAction, BuildingActionData upgradeAction, BuildingActionData produceUnitAction)
    {
        BuildingActionConfig config = ScriptableObject.CreateInstance<BuildingActionConfig>();
        config.actions = new BuildingActionData[] { sellAction, upgradeAction, produceUnitAction };
        config.selectionColor = new Color(0.2f, 1f, 0.2f); // Green
        config.outlineWidth = 3f;

        string path = folder + "/Config_Barracks.asset";
        AssetDatabase.CreateAsset(config, path);
    }

    private static void LinkActionConfigsToBuildingData()
    {
        // Load BuildingData assets
        BuildingData powerPlant = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_PowerPlant.asset");
        BuildingData refinery = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Refinery.asset");
        BuildingData barracks = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Barracks.asset");

        // Load ActionConfig assets
        BuildingActionConfig powerPlantConfig = AssetDatabase.LoadAssetAtPath<BuildingActionConfig>("Assets/ScriptableObjects/BuildingActionConfigs/Config_PowerPlant.asset");
        BuildingActionConfig refineryConfig = AssetDatabase.LoadAssetAtPath<BuildingActionConfig>("Assets/ScriptableObjects/BuildingActionConfigs/Config_Refinery.asset");
        BuildingActionConfig barracksConfig = AssetDatabase.LoadAssetAtPath<BuildingActionConfig>("Assets/ScriptableObjects/BuildingActionConfigs/Config_Barracks.asset");

        // Link configs to BuildingData
        if (powerPlant != null && powerPlantConfig != null)
        {
            powerPlant.actionConfig = powerPlantConfig;
            EditorUtility.SetDirty(powerPlant);
            Debug.Log("Linked Power Plant action config");
        }

        if (refinery != null && refineryConfig != null)
        {
            refinery.actionConfig = refineryConfig;
            EditorUtility.SetDirty(refinery);
            Debug.Log("Linked Refinery action config");
        }

        if (barracks != null && barracksConfig != null)
        {
            barracks.actionConfig = barracksConfig;
            EditorUtility.SetDirty(barracks);
            Debug.Log("Linked Barracks action config");
        }
    }

    private static void UpdateBuildingPrefabs()
    {
        // Load prefabs
        GameObject powerPlantPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BuildingPrefab_PowerPlant.prefab");
        GameObject refineryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BuildingPrefab_Refinery.prefab");
        GameObject barracksPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BuildingPrefab_Barracks.prefab");

        AddSelectionVisualToPrefab(powerPlantPrefab);
        AddSelectionVisualToPrefab(refineryPrefab);
        AddSelectionVisualToPrefab(barracksPrefab);

        Debug.Log("✅ Updated building prefabs with SelectionVisualController");
    }

    private static void AddSelectionVisualToPrefab(GameObject prefab)
    {
        if (prefab == null) return;

        string path = AssetDatabase.GetAssetPath(prefab);
        GameObject prefabInstance = PrefabUtility.LoadPrefabContents(path);

        // Check if SelectionVisualController already exists
        SelectionVisualController existingController = prefabInstance.GetComponent<SelectionVisualController>();
        if (existingController == null)
        {
            prefabInstance.AddComponent<SelectionVisualController>();
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
            Debug.Log($"Added SelectionVisualController to {prefab.name}");
        }
        else
        {
            Debug.Log($"SelectionVisualController already exists on {prefab.name}");
        }

        PrefabUtility.UnloadPrefabContents(prefabInstance);
    }
}
