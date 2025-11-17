using UnityEngine;
using UnityEditor;
using System.IO;
using RTS.Data;

public class CreateBuildingDataAssets
{
    [MenuItem("RTS/Create Test Building Data")]
    public static void CreateTestBuildingData()
    {
        string folderPath = "Assets/BuildingData";

        // Create folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "BuildingData");
        }

        // Create Power Plant
        BuildingData powerPlant = ScriptableObject.CreateInstance<BuildingData>();
        powerPlant.buildingName = "Power Plant";
        powerPlant.description = "Provides power to your base";
        powerPlant.creditsCost = 200;
        powerPlant.powerRequired = 0;
        powerPlant.buildTime = 5f;
        powerPlant.size = new Vector2Int(2, 2);
        powerPlant.requiresPower = false;
        powerPlant.providespower = true;
        powerPlant.powerProvided = 100;
        powerPlant.techLevel = 0;
        powerPlant.canProduceUnits = false;

        AssetDatabase.CreateAsset(powerPlant, folderPath + "/BuildingData_PowerPlant.asset");

        // Create Refinery
        BuildingData refinery = ScriptableObject.CreateInstance<BuildingData>();
        refinery.buildingName = "Refinery";
        refinery.description = "Generates credits over time";
        refinery.creditsCost = 500;
        refinery.powerRequired = 20;
        refinery.buildTime = 10f;
        refinery.size = new Vector2Int(3, 3);
        refinery.requiresPower = true;
        refinery.providespower = false;
        refinery.powerProvided = 0;
        refinery.techLevel = 0;
        refinery.canProduceUnits = false;

        AssetDatabase.CreateAsset(refinery, folderPath + "/BuildingData_Refinery.asset");

        // Create Barracks
        BuildingData barracks = ScriptableObject.CreateInstance<BuildingData>();
        barracks.buildingName = "Barracks";
        barracks.description = "Trains infantry units";
        barracks.creditsCost = 300;
        barracks.powerRequired = 10;
        barracks.buildTime = 8f;
        barracks.size = new Vector2Int(2, 2);
        barracks.requiresPower = true;
        barracks.providespower = false;
        barracks.powerProvided = 0;
        barracks.techLevel = 0;
        barracks.canProduceUnits = true;

        AssetDatabase.CreateAsset(barracks, folderPath + "/BuildingData_Barracks.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Created 3 test BuildingData assets in " + folderPath);
    }
}
