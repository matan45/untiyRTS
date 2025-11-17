using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RTS.Data;

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
            return;
        }

        // Load BuildingData assets
        BuildingData powerPlant = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_PowerPlant.asset");
        BuildingData refinery = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Refinery.asset");
        BuildingData barracks = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Barracks.asset");

        if (powerPlant == null || refinery == null || barracks == null)
        {
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

        }
        
    }
}
