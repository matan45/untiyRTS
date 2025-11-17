using UnityEngine;
using UnityEditor;
using RTS.Buildings;
using RTS.Data;

[InitializeOnLoad]
public class AutoAssignBuildingDataToPrefabs
{
    static AutoAssignBuildingDataToPrefabs()
    {
        EditorApplication.delayCall += AssignBuildingData;
    }

    static void AssignBuildingData()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;


        // Map of prefab names to BuildingData asset paths
        var mappings = new (string prefabName, string dataPath)[]
        {
            ("BuildingPrefab_Barracks", "Assets/BuildingData/BuildingData_Barracks.asset"),
            ("BuildingPrefab_PowerPlant", "Assets/BuildingData/BuildingData_PowerPlant.asset"),
            ("BuildingPrefab_Refinery", "Assets/BuildingData/BuildingData_Refinery.asset")
        };

        bool anyChanged = false;

        foreach (var (prefabName, dataPath) in mappings)
        {
            // Find prefab
            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
            if (guids.Length == 0)
            {
                continue;
            }

            string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab == null)
            {
                continue;
            }

            // Get Building component
            Building building = prefab.GetComponent<Building>();
            if (building == null)
            {
                continue;
            }

            // Load BuildingData
            BuildingData data = AssetDatabase.LoadAssetAtPath<BuildingData>(dataPath);
            if (data == null)
            {
                continue;
            }

            // Check if already assigned
            if (building.Data == data)
            {
                continue; // Already correct
            }

            // Assign using PrefabUtility to modify prefab properly
            string prefabAssetPath = AssetDatabase.GetAssetPath(prefab);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabAssetPath);
            Building buildingInPrefab = prefabContents.GetComponent<Building>();
            
            if (buildingInPrefab != null)
            {
                SerializedObject so = new SerializedObject(buildingInPrefab);
                so.FindProperty("buildingData").objectReferenceValue = data;
                so.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabAssetPath);
                PrefabUtility.UnloadPrefabContents(prefabContents);

                anyChanged = true;
            }
        }

        if (anyChanged)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AutoAssign] ✅ Building prefabs updated!");
        }
    }

    [MenuItem("RTS/Assign BuildingData to Prefabs")]
    public static void ManualAssign()
    {
        AssignBuildingData();
    }
}