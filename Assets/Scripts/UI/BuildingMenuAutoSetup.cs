using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Automatically wires up BuildingMenuController and BuildingButton references
/// </summary>
public class BuildingMenuAutoSetup : MonoBehaviour
{
    void Start()
    {
        SetupBuildingMenu();
        SetupBuildingButton();
    }

    [ContextMenu("Setup Building Menu")]
    void SetupBuildingMenu()
    {
        // Find BuildingMenuController
        BuildingMenuController menuController = FindFirstObjectByType<BuildingMenuController>();
        if (menuController == null)
        {
            return;
        }

        // Find UI elements
        Transform content = GameObject.Find("Content")?.transform;
        GameObject buttonPrefab = GameObject.Find("BuildingButtonPrefab");
        TextMeshProUGUI creditsText = GameObject.Find("CreditsText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI powerText = GameObject.Find("PowerText")?.GetComponent<TextMeshProUGUI>();
        
        // Use reflection to set private fields
        var type = typeof(BuildingMenuController);

        if (content != null)
        {
            var field = type.GetField("buildingButtonContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(menuController, content);
            }
        }

        if (buttonPrefab != null)
        {
            var field = type.GetField("buildingButtonPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(menuController, buttonPrefab);
            }
        }

        if (creditsText != null)
        {
            var field = type.GetField("creditsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(menuController, creditsText);
            }
        }

        if (powerText != null)
        {
            var field = type.GetField("powerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(menuController, powerText);
            }
        }

        // Re-initialize buttons now that references are set
        // Use reflection to call the private InitializeButtons method
        var initMethod = type.GetMethod("InitializeButtons", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (initMethod != null)
        {
            initMethod.Invoke(menuController, null);
        }
    }
    
    [ContextMenu("Setup Building Button")]
    void SetupBuildingButton()
    {
        GameObject buttonObj = GameObject.Find("BuildingButtonPrefab");
        if (buttonObj == null)
        {
            return;
        }
        
        BuildingButton button = buttonObj.GetComponent<BuildingButton>();
        if (button == null)
        {
            return;
        }
        
        // Find child elements
        Image iconImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
        TextMeshProUGUI nameText = buttonObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI costText = buttonObj.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
        Image lockOverlay = buttonObj.transform.Find("LockOverlay")?.GetComponent<Image>();
        Button btn = buttonObj.GetComponent<Button>();
        
        // Use reflection to set private fields
        var type = typeof(BuildingButton);
        
        if (iconImage != null)
        {
            var field = type.GetField("iconImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(button, iconImage);
        }
        
        if (nameText != null)
        {
            var field = type.GetField("nameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(button, nameText);
        }
        
        if (costText != null)
        {
            var field = type.GetField("costText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(button, costText);
        }
        
        if (lockOverlay != null)
        {
            var field = type.GetField("lockOverlay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(button, lockOverlay);
        }
        
        if (btn != null)
        {
            var field = type.GetField("button", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(button, btn);
        }
        
    }
}