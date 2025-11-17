using UnityEngine;
using UnityEditor;
using RTS.Data;

/// <summary>
/// Quick test tool to set different button sizes on BuildingActionConfig assets
/// </summary>
public class TestButtonSizing : EditorWindow
{
    [MenuItem("RTS/Test Button Sizing")]
    public static void ShowWindow()
    {
        GetWindow<TestButtonSizing>("Test Button Sizing");
    }

    private void OnGUI()
    {
        GUILayout.Label("Quick Button Size Testing", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This tool will set DRAMATICALLY DIFFERENT button sizes on your BuildingActionConfig assets " +
            "so you can easily see if the sizing system is working.\n\n" +
            "After applying, select a building in Play mode and check the Console for debug logs.",
            MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("Apply Test Sizes (Small/Medium/Large)", GUILayout.Height(40)))
        {
            ApplyTestSizes();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Reset All to Default (120x120)", GUILayout.Height(30)))
        {
            ResetToDefaults();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Set All to Large (150x150)", GUILayout.Height(30)))
        {
            SetAllToSize(new Vector2(150, 150));
        }

        GUILayout.Space(5);

        EditorGUILayout.HelpBox("Quick Fix: If buttons look wrong (too tall/wide), click below:", MessageType.Warning);

        if (GUILayout.Button("FIX: Set All to Square 120x120", GUILayout.Height(35)))
        {
            SetAllToSize(new Vector2(120, 120));
        }

        GUILayout.Space(10);

        GUILayout.Label("Current Config Sizes:", EditorStyles.boldLabel);
        ShowCurrentSizes();
    }

    private void ApplyTestSizes()
    {
        // Find all BuildingActionConfig assets
        string[] guids = AssetDatabase.FindAssets("t:BuildingActionConfig");

        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("No Configs Found", "No BuildingActionConfig assets found!", "OK");
            return;
        }

        // Define test sizes - larger and more suitable for RTS
        Vector2[] testSizes = new Vector2[]
        {
            new Vector2(90, 90),    // Compact
            new Vector2(120, 120),  // Standard
            new Vector2(150, 150),  // Large
            new Vector2(110, 140),  // Tall (good for unit production)
            new Vector2(180, 100),  // Wide (good for horizontal layouts)
        };

        int configIndex = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BuildingActionConfig config = AssetDatabase.LoadAssetAtPath<BuildingActionConfig>(path);

            if (config != null)
            {
                Undo.RecordObject(config, "Set Test Button Size");

                // Assign test size (cycle through sizes)
                config.buttonSize = testSizes[configIndex % testSizes.Length];

                // Vary spacing too
                config.buttonSpacing = 5f + (configIndex * 5f);

                // Vary layout type for extra testing
                config.layoutType = (ButtonLayoutType)(configIndex % 3);

                EditorUtility.SetDirty(config);

                Debug.Log($"✓ Set {config.name}: Size={config.buttonSize}, Layout={config.layoutType}, Spacing={config.buttonSpacing}");

                configIndex++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success",
            $"Applied test sizes to {configIndex} BuildingActionConfig assets!\n\n" +
            "Check Console for details.\n" +
            "Now test in Play mode by selecting different buildings.",
            "OK");
    }

    private void ResetToDefaults()
    {
        SetAllToSize(new Vector2(120f, 120f));
    }

    private void SetAllToSize(Vector2 size)
    {
        string[] guids = AssetDatabase.FindAssets("t:BuildingActionConfig");

        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BuildingActionConfig config = AssetDatabase.LoadAssetAtPath<BuildingActionConfig>(path);

            if (config != null)
            {
                Undo.RecordObject(config, "Set Button Size");

                config.buttonSize = size;
                config.buttonSpacing = 10f;
                config.layoutType = ButtonLayoutType.Grid;
                config.gridColumns = 3;

                EditorUtility.SetDirty(config);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Update Complete", $"Set {count} configs to size {size}.", "OK");
    }

    private void ShowCurrentSizes()
    {
        string[] guids = AssetDatabase.FindAssets("t:BuildingActionConfig");

        if (guids.Length == 0)
        {
            GUILayout.Label("No BuildingActionConfig assets found.", EditorStyles.helpBox);
            return;
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BuildingActionConfig config = AssetDatabase.LoadAssetAtPath<BuildingActionConfig>(path);

            if (config != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(config.name, GUILayout.Width(150));
                GUILayout.Label($"Size: {config.buttonSize}", GUILayout.Width(120));
                GUILayout.Label($"Layout: {config.layoutType}", GUILayout.Width(100));

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = config;
                    EditorGUIUtility.PingObject(config);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndVertical();
    }
}
