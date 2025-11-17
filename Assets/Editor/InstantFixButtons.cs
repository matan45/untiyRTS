using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using RTS.UI;
using RTS.Data;

public class InstantFixButtons
{
    [MenuItem("RTS/INSTANT FIX - Create Button Prefab")]
    public static void InstantFix()
    {

        // 1. Find ActionButton in scene
        GameObject actionButton = GameObject.Find("ActionButton");
        if (actionButton == null)
        {
            return;
        }

        // 2. Create folders
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

        // 3. Save as prefab
        string prefabPath = "Assets/Prefabs/UI/ActionButtonPrefab.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(actionButton, prefabPath);

        // 4. Find BuildingActionUIManager
        BuildingActionUIManager uiManager = Object.FindFirstObjectByType<BuildingActionUIManager>();
        if (uiManager == null)
        {
            return;
        }

        // 5. Assign prefab using SerializedObject
        SerializedObject so = new SerializedObject(uiManager);
        so.FindProperty("actionButtonPrefab").objectReferenceValue = prefab;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(uiManager);

        // 6. Create action data
        CreateActionDataQuick();

        // 7. Save everything
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    static void CreateActionDataQuick()
    {
        // Create folders
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/BuildingActions"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "BuildingActions");
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/BuildingActionConfigs"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "BuildingActionConfigs");

        // Create Sell action
        BuildingActionData sell = ScriptableObject.CreateInstance<BuildingActionData>();
        sell.actionId = "sell";
        sell.displayName = "Sell";
        sell.hotkey = Key.S;
        AssetDatabase.CreateAsset(sell, "Assets/ScriptableObjects/BuildingActions/Action_Sell.asset");

        // Create Upgrade action
        BuildingActionData upgrade = ScriptableObject.CreateInstance<BuildingActionData>();
        upgrade.actionId = "upgrade";
        upgrade.displayName = "Upgrade";
        upgrade.hotkey = Key.U;
        AssetDatabase.CreateAsset(upgrade, "Assets/ScriptableObjects/BuildingActions/Action_Upgrade.asset");

        // Create config for each building
        string[] buildingTypes = { "PowerPlant", "Refinery", "Barracks" };
        foreach (string type in buildingTypes)
        {
            BuildingActionConfig config = ScriptableObject.CreateInstance<BuildingActionConfig>();
            config.actions = new BuildingActionData[] { sell, upgrade };
            config.selectionColor = new Color(0.2f, 1f, 0.2f);
            config.outlineWidth = 3f;
            AssetDatabase.CreateAsset(config, $"Assets/ScriptableObjects/BuildingActionConfigs/Config_{type}.asset");

            // Link to BuildingData
            string dataPath = $"Assets/BuildingData/BuildingData_{type}.asset";
            BuildingData data = AssetDatabase.LoadAssetAtPath<BuildingData>(dataPath);
            if (data != null)
            {
                data.actionConfig = config;
                EditorUtility.SetDirty(data);
            }
        }
    }
}
