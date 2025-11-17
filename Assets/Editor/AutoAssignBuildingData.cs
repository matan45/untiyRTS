using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using RTS.Data;

[InitializeOnLoad]
public class AutoAssignBuildingData
{
    static AutoAssignBuildingData()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (scene.name != "SampleScene") return;

        EditorApplication.delayCall += () =>
        {
            // Find BuildingMenuController
            BuildingMenuController menuController = Object.FindFirstObjectByType<BuildingMenuController>();
            if (menuController == null) return;

            // Check if already assigned
            if (menuController.availableBuildings != null && menuController.availableBuildings.Count > 0)
            {
                Debug.Log("[AutoAssignBuildingData] BuildingData already assigned");
                return;
            }

            // Load BuildingData assets
            BuildingData powerPlant = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_PowerPlant.asset");
            BuildingData refinery = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Refinery.asset");
            BuildingData barracks = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Barracks.asset");

            if (powerPlant == null || refinery == null || barracks == null)
            {
                Debug.LogWarning("[AutoAssignBuildingData] Could not load BuildingData assets");
                return;
            }

            // Assign to list
            menuController.availableBuildings = new List<BuildingData> { powerPlant, refinery, barracks };

            // Save
            EditorUtility.SetDirty(menuController);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[AutoAssignBuildingData] Assigned 3 BuildingData assets and saved scene");
        };
    }
}
