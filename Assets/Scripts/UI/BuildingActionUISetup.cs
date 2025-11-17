using UnityEngine;
using RTS.UI;

/// <summary>
/// One-time setup script to wire up BuildingActionUIManager references.
/// Attach to BuildingActionPanel and it will auto-configure itself.
/// </summary>
[ExecuteInEditMode]
public class BuildingActionUISetup : MonoBehaviour
{
    private void Awake()
    {
        SetupReferences();
    }

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            SetupReferences();
        }
    }

    private void SetupReferences()
    {
        BuildingActionUIManager uiManager = GetComponent<BuildingActionUIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("BuildingActionUISetup: No BuildingActionUIManager found!");
            return;
        }

        // Use reflection to set private serialized fields
        var managerType = typeof(BuildingActionUIManager);

        // Set actionPanel to this GameObject
        var actionPanelField = managerType.GetField("actionPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (actionPanelField != null)
        {
            actionPanelField.SetValue(uiManager, gameObject);
            Debug.Log("Set actionPanel reference");
        }

        // Find and set buttonContainer
        Transform buttonContainer = transform.Find("ButtonContainer");
        if (buttonContainer != null)
        {
            var buttonContainerField = managerType.GetField("buttonContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (buttonContainerField != null)
            {
                buttonContainerField.SetValue(uiManager, buttonContainer);
                Debug.Log("Set buttonContainer reference");
            }
        }

        // Find and set infoDisplay
        BuildingInfoDisplay infoDisplay = transform.Find("InfoSection")?.GetComponent<BuildingInfoDisplay>();
        if (infoDisplay != null)
        {
            var infoDisplayField = managerType.GetField("infoDisplay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (infoDisplayField != null)
            {
                infoDisplayField.SetValue(uiManager, infoDisplay);
                Debug.Log("Set infoDisplay reference");
            }
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(uiManager);
#endif
    }
}
