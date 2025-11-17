using UnityEngine;
using UnityEditor;
using RTS.UI;
using RTS.Data;

public class FinalizeActionButton
{
    [MenuItem("RTS/Finalize Action Button Setup")]
    public static void Setup()
    {
        // 1. Find ActionButton in scene
        GameObject actionButtonObj = GameObject.Find("ActionButton");
        if (actionButtonObj == null)
        {
            Debug.LogError("ActionButton not found in scene!");
            return;
        }

        // 2. Create prefab folder if needed
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        // 3. Save as prefab
        string prefabPath = "Assets/Prefabs/UI/ActionButtonPrefab.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(actionButtonObj, prefabPath);
        Debug.Log("✅ Created ActionButtonPrefab");

        // 4. Delete from scene
        GameObject.DestroyImmediate(actionButtonObj);

        // 5. Find BuildingActionUIManager and assign prefab
        BuildingActionUIManager uiManager = GameObject.FindFirstObjectByType<BuildingActionUIManager>();
        if (uiManager != null)
        {
            SerializedObject serializedManager = new SerializedObject(uiManager);
            serializedManager.FindProperty("actionButtonPrefab").objectReferenceValue = prefab;
            serializedManager.ApplyModifiedProperties();
            EditorUtility.SetDirty(uiManager);
            Debug.Log("✅ Linked prefab to BuildingActionUIManager");
        }

        // 6. Create action data
        CreateActionData();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ All setup complete! Enter Play Mode and select a building!");
    }

    private static void CreateActionData()
    {
        // Create folders
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
        {
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        }
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/BuildingActions"))
        {
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "BuildingActions");
        }
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/BuildingActionConfigs"))
        {
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "BuildingActionConfigs");
        }

        // Create Sell action
        string sellPath = "Assets/ScriptableObjects/BuildingActions/Action_Sell.asset";
        BuildingActionData sellAction = AssetDatabase.LoadAssetAtPath<BuildingActionData>(sellPath);
        if (sellAction == null)
        {
            sellAction = ScriptableObject.CreateInstance<BuildingActionData>();
            sellAction.actionId = "sell";
            sellAction.displayName = "Sell";
            sellAction.tooltip = "Sell this building for 50% refund";
            sellAction.hotkey = KeyCode.S;
            AssetDatabase.CreateAsset(sellAction, sellPath);
            Debug.Log("Created Sell action");
        }

        // Create Upgrade action
        string upgradePath = "Assets/ScriptableObjects/BuildingActions/Action_Upgrade.asset";
        BuildingActionData upgradeAction = AssetDatabase.LoadAssetAtPath<BuildingActionData>(upgradePath);
        if (upgradeAction == null)
        {
            upgradeAction = ScriptableObject.CreateInstance<BuildingActionData>();
            upgradeAction.actionId = "upgrade";
            upgradeAction.displayName = "Upgrade";
            upgradeAction.tooltip = "Upgrade this building";
            upgradeAction.hotkey = KeyCode.U;
            AssetDatabase.CreateAsset(upgradeAction, upgradePath);
            Debug.Log("Created Upgrade action");
        }

        // Create configs for each building type
        CreateBuildingConfig("PowerPlant", sellAction, upgradeAction);
        CreateBuildingConfig("Refinery", sellAction, upgradeAction);
        CreateBuildingConfig("Barracks", sellAction, upgradeAction);
    }

    private static void CreateBuildingConfig(string buildingType, params BuildingActionData[] actions)
    {
        string configPath = $"Assets/ScriptableObjects/BuildingActionConfigs/Config_{buildingType}.asset";
        BuildingActionConfig config = AssetDatabase.LoadAssetAtPath<BuildingActionConfig>(configPath);

        if (config == null)
        {
            config = ScriptableObject.CreateInstance<BuildingActionConfig>();
            config.actions = actions;
            config.selectionColor = new Color(0.2f, 1f, 0.2f);
            config.outlineWidth = 3f;
            AssetDatabase.CreateAsset(config, configPath);
            Debug.Log($"Created config for {buildingType}");
        }

        // Link to BuildingData
        string buildingDataPath = $"Assets/BuildingData/BuildingData_{buildingType}.asset";
        BuildingData buildingData = AssetDatabase.LoadAssetAtPath<BuildingData>(buildingDataPath);
        if (buildingData != null && buildingData.actionConfig == null)
        {
            buildingData.actionConfig = config;
            EditorUtility.SetDirty(buildingData);
            Debug.Log($"Linked config to {buildingType} BuildingData");
        }
    }
}
