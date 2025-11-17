using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using RTS.Data;
using RTS.Buildings;

public class ForceSetupReferences : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("[ForceSetupReferences] Starting setup...");
        SetupEverything();
    }
    
    void SetupEverything()
    {
        // Setup BuildingMenuController
        BuildingMenuController menuController = FindFirstObjectByType<BuildingMenuController>();
        if (menuController != null)
        {
            Transform content = GameObject.Find("Content")?.transform;
            GameObject buttonPrefab = GameObject.Find("BuildingButtonPrefab");
            TextMeshProUGUI creditsText = GameObject.Find("CreditsText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI powerText = GameObject.Find("PowerText")?.GetComponent<TextMeshProUGUI>();
            
            var type = typeof(BuildingMenuController);
            
            if (content != null)
            {
                var field = type.GetField("buildingButtonContainer", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) 
                {
                    field.SetValue(menuController, content);
                    Debug.Log("[ForceSetupReferences] Set buildingButtonContainer");
                }
            }
            
            if (buttonPrefab != null)
            {
                var field = type.GetField("buildingButtonPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) 
                {
                    field.SetValue(menuController, buttonPrefab);
                    Debug.Log("[ForceSetupReferences] Set buildingButtonPrefab");
                }
            }
            
            if (creditsText != null)
            {
                var field = type.GetField("creditsText", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) 
                {
                    field.SetValue(menuController, creditsText);
                    Debug.Log("[ForceSetupReferences] Set creditsText");
                }
            }
            
            if (powerText != null)
            {
                var field = type.GetField("powerText", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(menuController, powerText);
                    Debug.Log("[ForceSetupReferences] Set powerText");
                }
            }

            // Load and set BuildingData assets
            #if UNITY_EDITOR
            var buildingDataList = new System.Collections.Generic.List<BuildingData>();
            buildingDataList.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_PowerPlant.asset"));
            buildingDataList.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Refinery.asset"));
            buildingDataList.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/BuildingData/BuildingData_Barracks.asset"));

            var availableBuildingsField = type.GetField("availableBuildings", BindingFlags.NonPublic | BindingFlags.Instance);
            if (availableBuildingsField != null)
            {
                availableBuildingsField.SetValue(menuController, buildingDataList);
                Debug.Log("[ForceSetupReferences] Set availableBuildings with 3 building data assets");
            }
            else
            {
                Debug.LogWarning("[ForceSetupReferences] Could not find availableBuildings field!");
            }
            #endif
        }
        
        // Setup BuildingButton
        GameObject buttonObj = GameObject.Find("BuildingButtonPrefab");
        if (buttonObj != null)
        {
            BuildingButton button = buttonObj.GetComponent<BuildingButton>();
            if (button != null)
            {
                Image iconImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
                TextMeshProUGUI nameText = buttonObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI costText = buttonObj.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
                Image lockOverlay = buttonObj.transform.Find("LockOverlay")?.GetComponent<Image>();
                Button btn = buttonObj.GetComponent<Button>();
                
                var type = typeof(BuildingButton);
                
                if (iconImage != null)
                {
                    var field = type.GetField("iconImage", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null) field.SetValue(button, iconImage);
                }
                
                if (nameText != null)
                {
                    var field = type.GetField("nameText", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null) field.SetValue(button, nameText);
                }
                
                if (costText != null)
                {
                    var field = type.GetField("costText", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null) field.SetValue(button, costText);
                }
                
                if (lockOverlay != null)
                {
                    var field = type.GetField("lockOverlay", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null) field.SetValue(button, lockOverlay);
                }
                
                if (btn != null)
                {
                    var field = type.GetField("button", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null) field.SetValue(button, btn);
                }
                
                Debug.Log("[ForceSetupReferences] BuildingButton setup complete!");
            }
        }
        
        Debug.Log("[ForceSetupReferences] Setup complete!");
    }
}