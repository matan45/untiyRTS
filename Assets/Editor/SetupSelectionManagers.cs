using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;
using System.Linq;
using RTS.Selection;
using RTS.Actions;

/// <summary>
/// Automatically sets up BuildingSelectionManager and BuildingActionExecutor in the scene.
/// </summary>
public class SetupSelectionManagers
{
    [MenuItem("RTS/Setup Selection Managers")]
    public static void SetupManagers()
    {
        // Find or create GameManagers object
        GameObject gameManagers = GameObject.Find("GameManagers");
        if (gameManagers == null)
        {
            return;
        }

        bool madeChanges = false;

        // Setup BuildingSelectionManager
        BuildingSelectionManager selectionManager = gameManagers.GetComponent<BuildingSelectionManager>();
        if (selectionManager == null)
        {
            selectionManager = gameManagers.AddComponent<BuildingSelectionManager>();
            madeChanges = true;
        }

        // Load and assign InputActionAsset
        InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Scripts/RTSInputActions.inputactions");
        if (inputActions == null)
        {
            // Try alternative path
            inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/RTSInputActions.inputactions");
        }

        if (inputActions != null)
        {
            // Use SerializedObject to set private field
            SerializedObject serializedManager = new SerializedObject(selectionManager);
            SerializedProperty inputActionsProperty = serializedManager.FindProperty("inputActions");

            if (inputActionsProperty != null)
            {
                inputActionsProperty.objectReferenceValue = inputActions;
                serializedManager.ApplyModifiedProperties();
                madeChanges = true;
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find inputActions field on BuildingSelectionManager");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Could not find RTSInputActions.inputactions asset");
        }

        // Setup BuildingActionExecutor
        BuildingActionExecutor actionExecutor = gameManagers.GetComponent<BuildingActionExecutor>();
        if (actionExecutor == null)
        {
            gameManagers.AddComponent<BuildingActionExecutor>();
            Debug.Log("✅ Added BuildingActionExecutor to GameManagers");
            madeChanges = true;
        }
        else
        {
            Debug.Log("ℹ️ BuildingActionExecutor already exists on GameManagers");
        }

        if (madeChanges)
        {
            // Mark scene dirty and save
            EditorUtility.SetDirty(gameManagers);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("✅ Selection Managers setup complete! Don't forget to save the scene.");
        }
        else
        {
            Debug.Log("ℹ️ Managers already set up correctly");
        }
    }

    [MenuItem("RTS/Verify Selection System Setup")]
    public static void VerifySetup()
    {
        Debug.Log("=== SELECTION SYSTEM VERIFICATION ===");

        // Check GameManagers
        GameObject gameManagers = GameObject.Find("GameManagers");
        if (gameManagers == null)
        {
            Debug.LogError("❌ GameManagers not found in scene");
            return;
        }

        // Check BuildingSelectionManager
        BuildingSelectionManager selectionManager = gameManagers.GetComponent<BuildingSelectionManager>();
        if (selectionManager == null)
        {
            Debug.LogError("❌ BuildingSelectionManager not found on GameManagers");
        }
        else
        {
            Debug.Log("✅ BuildingSelectionManager found");

            // Check InputActionAsset
            SerializedObject serializedManager = new SerializedObject(selectionManager);
            SerializedProperty inputActionsProperty = serializedManager.FindProperty("inputActions");

            if (inputActionsProperty != null && inputActionsProperty.objectReferenceValue != null)
            {
                Debug.Log("✅ InputActionAsset assigned");
            }
            else
            {
                Debug.LogError("❌ InputActionAsset NOT assigned");
            }
        }

        // Check BuildingActionExecutor
        BuildingActionExecutor actionExecutor = gameManagers.GetComponent<BuildingActionExecutor>();
        if (actionExecutor == null)
        {
            Debug.LogError("❌ BuildingActionExecutor not found on GameManagers");
        }
        else
        {
            Debug.Log("✅ BuildingActionExecutor found");
        }

        // Check for buildings in scene
        RTS.Buildings.Building[] buildings = GameObject.FindObjectsByType<RTS.Buildings.Building>(FindObjectsSortMode.None);
        Debug.Log($"ℹ️ Found {buildings.Length} buildings in scene");

        // Check BuildingData assets
        RTS.Data.BuildingData[] buildingDataAssets = AssetDatabase.FindAssets("t:BuildingData")
            .Select(guid => AssetDatabase.LoadAssetAtPath<RTS.Data.BuildingData>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToArray();

        int configuredCount = 0;
        foreach (var data in buildingDataAssets)
        {
            if (data.actionConfig != null)
            {
                configuredCount++;
                Debug.Log($"✅ {data.buildingName} has action config");
            }
            else
            {
                Debug.LogWarning($"⚠️ {data.buildingName} missing action config");
            }
        }

        Debug.Log($"=== {configuredCount}/{buildingDataAssets.Length} BuildingData assets configured ===");
    }
}
