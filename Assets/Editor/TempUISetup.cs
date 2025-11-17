using UnityEngine;
using UnityEditor;
using RTS.UI;

public class TempUISetup
{
    [MenuItem("RTS/Fix UI References Now")]
    public static void FixReferences()
    {
        BuildingActionUIManager uiManager = GameObject.FindFirstObjectByType<BuildingActionUIManager>();
        if (uiManager == null)
        {
            Debug.LogError("BuildingActionUIManager not found!");
            return;
        }

        SerializedObject serializedManager = new SerializedObject(uiManager);
        
        // Set actionPanel to the GameObject itself
        serializedManager.FindProperty("actionPanel").objectReferenceValue = uiManager.gameObject;
        
        // Find and set buttonContainer
        Transform buttonContainer = uiManager.transform.Find("ButtonContainer");
        if (buttonContainer != null)
        {
            serializedManager.FindProperty("buttonContainer").objectReferenceValue = buttonContainer;
        }
        
        // Find and set infoDisplay
        BuildingInfoDisplay infoDisplay = uiManager.transform.Find("InfoSection")?.GetComponent<BuildingInfoDisplay>();
        if (infoDisplay != null)
        {
            serializedManager.FindProperty("infoDisplay").objectReferenceValue = infoDisplay;
        }
        
        serializedManager.ApplyModifiedProperties();
        EditorUtility.SetDirty(uiManager);
        
        Debug.Log("UI references fixed!");
    }
}
