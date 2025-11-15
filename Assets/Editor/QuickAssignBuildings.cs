using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

// Quick menu item to assign BuildingData assets
public static class QuickAssignBuildings
{
    [MenuItem("RTS/Quick Assign Buildings to Menu")]
    public static void AssignBuildings()
    {
        // Find BuildingMenuController
        BuildingMenuController menuController = Object.FindFirstObjectByType<BuildingMenuController>();
        if (menuController == null)
        {
            Debug.LogError("BuildingMenuController not found in scene!");
            return;
        }

        // Load BuildingData assets
        BuildingData powerPlant = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_PowerPlant.asset");
        BuildingData refinery = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Refinery.asset");
        BuildingData barracks = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Barracks.asset");

        if (powerPlant == null || refinery == null || barracks == null)
        {
            Debug.LogError("Could not load one or more BuildingData assets!");
            return;
        }

        // Assign to list
        menuController.availableBuildings = new List<BuildingData> { powerPlant, refinery, barracks };

        // Save
        EditorUtility.SetDirty(menuController);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        Debug.Log("✓ Successfully assigned 3 BuildingData assets to BuildingMenuController and saved scene!");
    }
}
