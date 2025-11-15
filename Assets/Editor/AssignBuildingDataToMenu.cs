using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Auto-assigns BuildingData assets to BuildingMenuController
public class AssignBuildingDataToMenu
{
    [MenuItem("RTS/Assign BuildingData to Menu")]
    public static void AssignBuildingData()
    {

        // Find BuildingMenuController in the scene
        BuildingMenuController menuController = GameObject.FindFirstObjectByType<BuildingMenuController>();
        if (menuController == null)
        {
            Debug.LogWarning("[AssignBuildingDataToMenu] BuildingMenuController not found in scene");
            return;
        }

        // Load BuildingData assets
        BuildingData powerPlant = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_PowerPlant.asset");
        BuildingData refinery = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Refinery.asset");
        BuildingData barracks = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Barracks.asset");

        if (powerPlant == null || refinery == null || barracks == null)
        {
            Debug.LogWarning("[AssignBuildingDataToMenu] One or more BuildingData assets not found");
            return;
        }

        // Assign to availableBuildings list using reflection
        var buildingDataList = new System.Collections.Generic.List<BuildingData> { powerPlant, refinery, barracks };

        var type = typeof(BuildingMenuController);
        var field = type.GetField("availableBuildings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(menuController, buildingDataList);

            // Mark scene as dirty and save
            EditorUtility.SetDirty(menuController);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("[AssignBuildingDataToMenu] Successfully assigned 3 BuildingData assets to BuildingMenuController and saved scene");
        }
        else
        {
            Debug.LogError("[AssignBuildingDataToMenu] Could not find availableBuildings field");
        }
    }
}
