using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RTS.UI;
using RTS.Data;

[InitializeOnLoad]
public class AutoFixActionButton
{
    static AutoFixActionButton()
    {
        // Run after a delay to ensure scene is loaded
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            DoFix();
        };
    }

    static void DoFix()
    {
        // Check if prefab already exists
        if (AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/ActionButtonPrefab.prefab") != null)
        {
            Debug.Log("[AutoFix] Prefab already exists, skipping...");
            return;
        }

        Debug.Log("[AutoFix] Starting action button fix...");

        // Find ActionButton
        GameObject actionButton = GameObject.Find("ActionButton");
        if (actionButton == null)
        {
            Debug.LogWarning("[AutoFix] ActionButton not found in scene");
            return;
        }

        // Create folders
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

        // Save as prefab
        string prefabPath = "Assets/Prefabs/UI/ActionButtonPrefab.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(actionButton, prefabPath);
        Debug.Log($"[AutoFix] ✅ Created {prefabPath}");

        // Find UI Manager and assign prefab
        BuildingActionUIManager uiManager = Object.FindFirstObjectByType<BuildingActionUIManager>();
        if (uiManager != null)
        {
            SerializedObject so = new SerializedObject(uiManager);
            so.FindProperty("actionButtonPrefab").objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(uiManager);
            Debug.Log("[AutoFix] ✅ Assigned prefab to UI Manager");
        }

        // Create action data
        CreateData();

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        Debug.Log("[AutoFix] ✅ ALL DONE! Enter Play Mode and select a building!");
    }

    static void CreateData()
    {
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/BuildingActions"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "BuildingActions");

        string sellPath = "Assets/ScriptableObjects/BuildingActions/Action_Sell.asset";
        if (AssetDatabase.LoadAssetAtPath<BuildingActionData>(sellPath) == null)
        {
            BuildingActionData sell = ScriptableObject.CreateInstance<BuildingActionData>();
            sell.actionId = "sell";
            sell.displayName = "Sell";
            sell.tooltip = "Sell building for 50% refund";
            sell.hotkey = KeyCode.S;
            AssetDatabase.CreateAsset(sell, sellPath);
            Debug.Log("[AutoFix] Created Sell action");
        }

        string upgradePath = "Assets/ScriptableObjects/BuildingActions/Action_Upgrade.asset";
        if (AssetDatabase.LoadAssetAtPath<BuildingActionData>(upgradePath) == null)
        {
            BuildingActionData upgrade = ScriptableObject.CreateInstance<BuildingActionData>();
            upgrade.actionId = "upgrade";
            upgrade.displayName = "Upgrade";
            upgrade.tooltip = "Upgrade building";
            upgrade.hotkey = KeyCode.U;
            AssetDatabase.CreateAsset(upgrade, upgradePath);
            Debug.Log("[AutoFix] Created Upgrade action");
        }

        // Load actions
        BuildingActionData sellAction = AssetDatabase.LoadAssetAtPath<BuildingActionData>(sellPath);
        BuildingActionData upgradeAction = AssetDatabase.LoadAssetAtPath<BuildingActionData>(upgradePath);

        // Create configs
        string[] types = { "PowerPlant", "Refinery", "Barracks" };
        foreach (string type in types)
        {
            string configPath = $"Assets/ScriptableObjects/BuildingActionConfigs/Config_{type}.asset";
            if (AssetDatabase.LoadAssetAtPath<BuildingActionConfig>(configPath) == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/BuildingActionConfigs"))
                    AssetDatabase.CreateFolder("Assets/ScriptableObjects", "BuildingActionConfigs");

                BuildingActionConfig config = ScriptableObject.CreateInstance<BuildingActionConfig>();
                config.actions = new BuildingActionData[] { sellAction, upgradeAction };
                config.selectionColor = Color.green;
                config.outlineWidth = 3f;
                AssetDatabase.CreateAsset(config, configPath);

                // Link to BuildingData
                BuildingData data = AssetDatabase.LoadAssetAtPath<BuildingData>($"Assets/BuildingData/BuildingData_{type}.asset");
                if (data != null)
                {
                    data.actionConfig = config;
                    EditorUtility.SetDirty(data);
                    Debug.Log($"[AutoFix] ✅ Configured {type}");
                }
            }
        }
    }
}
