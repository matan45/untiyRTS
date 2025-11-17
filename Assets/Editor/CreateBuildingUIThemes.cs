using UnityEngine;
using UnityEditor;
using RTS.Data;

/// <summary>
/// Creates example BuildingActionConfig assets with different visual themes
/// </summary>
public class CreateBuildingUIThemes : EditorWindow
{
    [MenuItem("RTS/Create Building UI Themes")]
    public static void ShowWindow()
    {
        if (EditorUtility.DisplayDialog("Create Building UI Themes",
            "This will create example BuildingActionConfig assets with different visual themes for:\n\n" +
            "• Power Plant (Blue theme, 2-column grid)\n" +
            "• Barracks (Green theme, vertical list)\n" +
            "• Refinery (Gold theme, horizontal)\n" +
            "• War Factory (Red theme, 3-column grid)\n\n" +
            "Continue?", "Create Themes", "Cancel"))
        {
            CreateThemes();
        }
    }

    private static void CreateThemes()
    {
        // Ensure folders exist
        CreateFolderIfNotExists("Assets/ScriptableObjects");
        CreateFolderIfNotExists("Assets/ScriptableObjects/BuildingActionConfigs");
        CreateFolderIfNotExists("Assets/ScriptableObjects/BuildingActions");

        // Create basic actions (Sell, Upgrade) if they don't exist
        CreateBasicActions();

        // Create themed configs
        CreatePowerPlantTheme();
        CreateBarracksTheme();
        CreateRefineryTheme();
        CreateWarFactoryTheme();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ Building UI Themes created successfully!");
        EditorUtility.DisplayDialog("Success",
            "Building UI Themes created!\n\nCheck Assets/ScriptableObjects/BuildingActionConfigs/\n\n" +
            "You can now assign these configs to your buildings and customize them further.",
            "OK");
    }

    private static void CreateBasicActions()
    {
        string sellPath = "Assets/ScriptableObjects/BuildingActions/Action_Sell.asset";
        if (AssetDatabase.LoadAssetAtPath<BuildingActionData>(sellPath) == null)
        {
            BuildingActionData sell = ScriptableObject.CreateInstance<BuildingActionData>();
            sell.actionId = "sell";
            sell.displayName = "Sell";
            sell.creditsCost = 0;
            sell.powerCost = 0;
            sell.hotkey = UnityEngine.InputSystem.Key.S;
            AssetDatabase.CreateAsset(sell, sellPath);
            Debug.Log("Created Sell action");
        }

        string upgradePath = "Assets/ScriptableObjects/BuildingActions/Action_Upgrade.asset";
        if (AssetDatabase.LoadAssetAtPath<BuildingActionData>(upgradePath) == null)
        {
            BuildingActionData upgrade = ScriptableObject.CreateInstance<BuildingActionData>();
            upgrade.actionId = "upgrade";
            upgrade.displayName = "Upgrade";
            upgrade.creditsCost = 0;
            upgrade.powerCost = 0;
            upgrade.hotkey = UnityEngine.InputSystem.Key.U;
            AssetDatabase.CreateAsset(upgrade, upgradePath);
            Debug.Log("Created Upgrade action");
        }
    }

    private static void CreatePowerPlantTheme()
    {
        string path = "Assets/ScriptableObjects/BuildingActionConfigs/Config_PowerPlant_Themed.asset";

        BuildingActionConfig config = ScriptableObject.CreateInstance<BuildingActionConfig>();

        // Load actions
        BuildingActionData sell = AssetDatabase.LoadAssetAtPath<BuildingActionData>("Assets/ScriptableObjects/BuildingActions/Action_Sell.asset");
        BuildingActionData upgrade = AssetDatabase.LoadAssetAtPath<BuildingActionData>("Assets/ScriptableObjects/BuildingActions/Action_Upgrade.asset");
        config.actions = new BuildingActionData[] { sell, upgrade };

        // Selection visual
        config.selectionColor = new Color(0.2f, 0.5f, 1f, 0.5f); // Blue
        config.outlineWidth = 3f;

        // UI Customization - Blue Theme
        config.panelBackgroundColor = new Color(0.1f, 0.2f, 0.4f, 0.95f); // Dark blue
        config.headerColor = new Color(0.2f, 0.3f, 0.5f, 1f); // Medium blue
        config.accentColor = new Color(0f, 0.8f, 1f, 1f); // Electric blue

        // Button Layout - Compact Grid
        config.layoutType = ButtonLayoutType.Grid;
        config.gridColumns = 2;
        config.buttonSize = new Vector2(100f, 100f);
        config.buttonSpacing = 10f;
        config.buttonContainerPadding = new Vector4(10f, 10f, 10f, 10f);

        AssetDatabase.CreateAsset(config, path);
        Debug.Log($"Created Power Plant theme at {path}");
    }

    private static void CreateBarracksTheme()
    {
        string path = "Assets/ScriptableObjects/BuildingActionConfigs/Config_Barracks_Themed.asset";

        BuildingActionConfig config = ScriptableObject.CreateInstance<BuildingActionConfig>();

        // Load actions
        BuildingActionData sell = AssetDatabase.LoadAssetAtPath<BuildingActionData>("Assets/ScriptableObjects/BuildingActions/Action_Sell.asset");
        BuildingActionData upgrade = AssetDatabase.LoadAssetAtPath<BuildingActionData>("Assets/ScriptableObjects/BuildingActions/Action_Upgrade.asset");
        config.actions = new BuildingActionData[] { sell, upgrade };

        // Selection visual
        config.selectionColor = new Color(0.2f, 1f, 0.2f, 0.5f); // Green
        config.outlineWidth = 3f;

        // UI Customization - Green Theme
        config.panelBackgroundColor = new Color(0.1f, 0.3f, 0.1f, 0.95f); // Dark green
        config.headerColor = new Color(0.2f, 0.4f, 0.2f, 1f); // Medium green
        config.accentColor = new Color(0f, 1f, 0.3f, 1f); // Bright green

        // Button Layout - Vertical List (good for unit production)
        config.layoutType = ButtonLayoutType.Vertical;
        config.buttonSize = new Vector2(150f, 60f); // Wide buttons
        config.buttonSpacing = 5f;
        config.buttonContainerPadding = new Vector4(20f, 20f, 10f, 10f);

        AssetDatabase.CreateAsset(config, path);
        Debug.Log($"Created Barracks theme at {path}");
    }

    private static void CreateRefineryTheme()
    {
        string path = "Assets/ScriptableObjects/BuildingActionConfigs/Config_Refinery_Themed.asset";

        BuildingActionConfig config = ScriptableObject.CreateInstance<BuildingActionConfig>();

        // Load actions
        BuildingActionData sell = AssetDatabase.LoadAssetAtPath<BuildingActionData>("Assets/ScriptableObjects/BuildingActions/Action_Sell.asset");
        BuildingActionData upgrade = AssetDatabase.LoadAssetAtPath<BuildingActionData>("Assets/ScriptableObjects/BuildingActions/Action_Upgrade.asset");
        config.actions = new BuildingActionData[] { sell, upgrade };

        // Selection visual
        config.selectionColor = new Color(1f, 0.8f, 0.2f, 0.5f); // Gold
        config.outlineWidth = 3f;

        // UI Customization - Gold Theme
        config.panelBackgroundColor = new Color(0.3f, 0.2f, 0.1f, 0.95f); // Dark gold
        config.headerColor = new Color(0.5f, 0.4f, 0.2f, 1f); // Medium gold
        config.accentColor = new Color(1f, 0.8f, 0f, 1f); // Bright gold

        // Button Layout - Horizontal (simple actions)
        config.layoutType = ButtonLayoutType.Horizontal;
        config.buttonSize = new Vector2(80f, 80f); // Smaller square buttons
        config.buttonSpacing = 15f;
        config.buttonContainerPadding = new Vector4(10f, 10f, 10f, 10f);

        AssetDatabase.CreateAsset(config, path);
        Debug.Log($"Created Refinery theme at {path}");
    }

    private static void CreateWarFactoryTheme()
    {
        string path = "Assets/ScriptableObjects/BuildingActionConfigs/Config_WarFactory_Themed.asset";

        BuildingActionConfig config = ScriptableObject.CreateInstance<BuildingActionConfig>();

        // Load actions
        BuildingActionData sell = AssetDatabase.LoadAssetAtPath<BuildingActionData>("Assets/ScriptableObjects/BuildingActions/Action_Sell.asset");
        BuildingActionData upgrade = AssetDatabase.LoadAssetAtPath<BuildingActionData>("Assets/ScriptableObjects/BuildingActions/Action_Upgrade.asset");
        config.actions = new BuildingActionData[] { sell, upgrade };

        // Selection visual
        config.selectionColor = new Color(1f, 0.2f, 0.2f, 0.5f); // Red
        config.outlineWidth = 3f;

        // UI Customization - Red Theme
        config.panelBackgroundColor = new Color(0.3f, 0.1f, 0.1f, 0.95f); // Dark red
        config.headerColor = new Color(0.5f, 0.2f, 0.2f, 1f); // Medium red
        config.accentColor = new Color(1f, 0.2f, 0f, 1f); // Bright red

        // Button Layout - Large Grid (many vehicle types)
        config.layoutType = ButtonLayoutType.Grid;
        config.gridColumns = 3;
        config.buttonSize = new Vector2(90f, 90f);
        config.buttonSpacing = 8f;
        config.buttonContainerPadding = new Vector4(15f, 15f, 15f, 15f);

        AssetDatabase.CreateAsset(config, path);
        Debug.Log($"Created War Factory theme at {path}");
    }

    private static void CreateFolderIfNotExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parentFolder = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string folderName = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }
    }
}
