using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using TMPro;
using RTS.UI;

/// <summary>
/// Automatically creates the Building Selection UI panel hierarchy in the scene.
/// </summary>
public class CreateBuildingSelectionUI
{
    [MenuItem("RTS/Create Building Selection UI")]
    public static void CreateUI()
    {
        // Find Canvas
        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        // Check if UI already exists
        Transform existingPanel = canvas.transform.Find("BuildingActionPanel");
        if (existingPanel != null)
        {
            return;
        }

        // Create main panel
        GameObject panel = CreateBuildingActionPanel(canvas.transform);

        // Create info display section
        GameObject infoSection = CreateInfoSection(panel.transform);

        // Create button container
        GameObject buttonContainer = CreateButtonContainer(panel.transform);

        // Create action button prefab
        CreateActionButtonPrefab();

        // Mark scene dirty
        EditorUtility.SetDirty(canvas.gameObject);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        // Select the created panel
        Selection.activeGameObject = panel;
    }

    private static GameObject CreateBuildingActionPanel(Transform parent)
    {
        GameObject panel = new GameObject("BuildingActionPanel");
        panel.transform.SetParent(parent, false);

        // Add RectTransform
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 20f);
        rect.sizeDelta = new Vector2(800f, 150f);

        // Add Image background
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // Add CanvasGroup
        CanvasGroup canvasGroup = panel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Add BuildingActionUIManager
        BuildingActionUIManager uiManager = panel.AddComponent<BuildingActionUIManager>();

        Debug.Log("Created BuildingActionPanel");
        return panel;
    }

    private static GameObject CreateInfoSection(Transform parent)
    {
        GameObject infoSection = new GameObject("InfoSection");
        infoSection.transform.SetParent(parent, false);

        RectTransform rect = infoSection.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0.3f, 1f);
        rect.offsetMin = new Vector2(10f, 10f);
        rect.offsetMax = new Vector2(-10f, -10f);

        // Add BuildingInfoDisplay component
        BuildingInfoDisplay infoDisplay = infoSection.AddComponent<BuildingInfoDisplay>();

        // Create Building Name text
        GameObject nameTextObj = new GameObject("BuildingNameText");
        nameTextObj.transform.SetParent(infoSection.transform, false);
        RectTransform nameRect = nameTextObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.7f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "Building Name";
        nameText.fontSize = 18;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Left;

        // Create Status text
        GameObject statusTextObj = new GameObject("StatusText");
        statusTextObj.transform.SetParent(infoSection.transform, false);
        RectTransform statusRect = statusTextObj.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0f, 0.5f);
        statusRect.anchorMax = new Vector2(1f, 0.7f);
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;

        TextMeshProUGUI statusText = statusTextObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "Status: Ready";
        statusText.fontSize = 14;
        statusText.color = Color.green;
        statusText.alignment = TextAlignmentOptions.Left;

        // Create Health Bar background
        GameObject healthBarBg = new GameObject("HealthBarBackground");
        healthBarBg.transform.SetParent(infoSection.transform, false);
        RectTransform healthBgRect = healthBarBg.AddComponent<RectTransform>();
        healthBgRect.anchorMin = new Vector2(0f, 0.3f);
        healthBgRect.anchorMax = new Vector2(1f, 0.4f);
        healthBgRect.offsetMin = Vector2.zero;
        healthBgRect.offsetMax = Vector2.zero;

        Image healthBgImage = healthBarBg.AddComponent<Image>();
        healthBgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        // Create Health Bar fill
        GameObject healthBarFill = new GameObject("HealthBarFill");
        healthBarFill.transform.SetParent(healthBarBg.transform, false);
        RectTransform healthFillRect = healthBarFill.AddComponent<RectTransform>();
        healthFillRect.anchorMin = Vector2.zero;
        healthFillRect.anchorMax = Vector2.one;
        healthFillRect.offsetMin = new Vector2(2f, 2f);
        healthFillRect.offsetMax = new Vector2(-2f, -2f);

        Image healthFillImage = healthBarFill.AddComponent<Image>();
        healthFillImage.color = Color.green;
        healthFillImage.type = Image.Type.Filled;
        healthFillImage.fillMethod = Image.FillMethod.Horizontal;

        // Create Construction Bar (initially hidden)
        GameObject constructionBarBg = new GameObject("ConstructionBarBackground");
        constructionBarBg.transform.SetParent(infoSection.transform, false);
        RectTransform constructionBgRect = constructionBarBg.AddComponent<RectTransform>();
        constructionBgRect.anchorMin = new Vector2(0f, 0.1f);
        constructionBgRect.anchorMax = new Vector2(1f, 0.2f);
        constructionBgRect.offsetMin = Vector2.zero;
        constructionBgRect.offsetMax = Vector2.zero;

        Image constructionBgImage = constructionBarBg.AddComponent<Image>();
        constructionBgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        GameObject constructionBarFill = new GameObject("ConstructionBarFill");
        constructionBarFill.transform.SetParent(constructionBarBg.transform, false);
        RectTransform constructionFillRect = constructionBarFill.AddComponent<RectTransform>();
        constructionFillRect.anchorMin = Vector2.zero;
        constructionFillRect.anchorMax = Vector2.one;
        constructionFillRect.offsetMin = new Vector2(2f, 2f);
        constructionFillRect.offsetMax = new Vector2(-2f, -2f);

        Image constructionFillImage = constructionBarFill.AddComponent<Image>();
        constructionFillImage.color = Color.yellow;
        constructionFillImage.type = Image.Type.Filled;
        constructionFillImage.fillMethod = Image.FillMethod.Horizontal;

        constructionBarBg.SetActive(false);

        // Link references using SerializedObject
        SerializedObject serializedInfo = new SerializedObject(infoDisplay);
        serializedInfo.FindProperty("buildingNameText").objectReferenceValue = nameText;
        serializedInfo.FindProperty("statusText").objectReferenceValue = statusText;
        serializedInfo.FindProperty("healthBarFill").objectReferenceValue = healthFillImage;
        serializedInfo.FindProperty("constructionBarFill").objectReferenceValue = constructionFillImage;
        serializedInfo.FindProperty("constructionBarObject").objectReferenceValue = constructionBarBg;
        serializedInfo.ApplyModifiedProperties();

        Debug.Log("Created InfoSection with BuildingInfoDisplay");
        return infoSection;
    }

    private static GameObject CreateButtonContainer(Transform parent)
    {
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(parent, false);

        RectTransform rect = buttonContainer.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.3f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(10f, 10f);
        rect.offsetMax = new Vector2(-10f, -10f);

        // Add HorizontalLayoutGroup
        HorizontalLayoutGroup layout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Debug.Log("Created ButtonContainer with HorizontalLayoutGroup");
        return buttonContainer;
    }

    private static void CreateActionButtonPrefab()
    {
        // Create prefab folder if needed
        string prefabFolder = "Assets/Prefabs/UI";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        string prefabPath = prefabFolder + "/ActionButtonPrefab.prefab";

        // Check if prefab already exists
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            Debug.Log("ℹ️ ActionButtonPrefab already exists");
            return;
        }

        // Create button GameObject
        GameObject buttonObj = new GameObject("ActionButton");
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100f, 120f);

        // Add Image
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Add Button component
        Button button = buttonObj.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;

        // Add BuildingActionButton component
        BuildingActionButton actionButton = buttonObj.AddComponent<BuildingActionButton>();

        // Create Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(buttonObj.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.4f);
        iconRect.anchorMax = new Vector2(0.9f, 0.9f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = Color.white;

        // Create Name Text
        GameObject nameTextObj = new GameObject("NameText");
        nameTextObj.transform.SetParent(buttonObj.transform, false);
        RectTransform nameRect = nameTextObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.2f);
        nameRect.anchorMax = new Vector2(1f, 0.4f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "Action";
        nameText.fontSize = 12;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Center;

        // Create Cost Text
        GameObject costTextObj = new GameObject("CostText");
        costTextObj.transform.SetParent(buttonObj.transform, false);
        RectTransform costRect = costTextObj.AddComponent<RectTransform>();
        costRect.anchorMin = new Vector2(0f, 0.05f);
        costRect.anchorMax = new Vector2(1f, 0.2f);
        costRect.offsetMin = Vector2.zero;
        costRect.offsetMax = Vector2.zero;

        TextMeshProUGUI costText = costTextObj.AddComponent<TextMeshProUGUI>();
        costText.text = "$0";
        costText.fontSize = 10;
        costText.color = Color.yellow;
        costText.alignment = TextAlignmentOptions.Center;

        // Create Hotkey Text
        GameObject hotkeyTextObj = new GameObject("HotkeyText");
        hotkeyTextObj.transform.SetParent(buttonObj.transform, false);
        RectTransform hotkeyRect = hotkeyTextObj.AddComponent<RectTransform>();
        hotkeyRect.anchorMin = new Vector2(0.7f, 0.7f);
        hotkeyRect.anchorMax = new Vector2(0.95f, 0.95f);
        hotkeyRect.offsetMin = Vector2.zero;
        hotkeyRect.offsetMax = Vector2.zero;

        TextMeshProUGUI hotkeyText = hotkeyTextObj.AddComponent<TextMeshProUGUI>();
        hotkeyText.text = "Q";
        hotkeyText.fontSize = 14;
        hotkeyText.fontStyle = FontStyles.Bold;
        hotkeyText.color = Color.white;
        hotkeyText.alignment = TextAlignmentOptions.Center;

        // Link references
        SerializedObject serializedButton = new SerializedObject(actionButton);
        serializedButton.FindProperty("actionIcon").objectReferenceValue = iconImage;
        serializedButton.FindProperty("actionNameText").objectReferenceValue = nameText;
        serializedButton.FindProperty("costText").objectReferenceValue = costText;
        serializedButton.FindProperty("hotkeyText").objectReferenceValue = hotkeyText;
        serializedButton.FindProperty("button").objectReferenceValue = button;
        serializedButton.ApplyModifiedProperties();

        // Save as prefab
        PrefabUtility.SaveAsPrefabAsset(buttonObj, prefabPath);
        Object.DestroyImmediate(buttonObj);

        Debug.Log($"✅ Created ActionButtonPrefab at {prefabPath}");
    }

    [MenuItem("RTS/Link Action Button Prefab")]
    public static void LinkActionButtonPrefab()
    {
        // Find BuildingActionUIManager
        BuildingActionUIManager uiManager = GameObject.FindFirstObjectByType<BuildingActionUIManager>();
        if (uiManager == null)
        {
            Debug.LogError("❌ BuildingActionUIManager not found in scene");
            return;
        }

        // Load ActionButtonPrefab
        GameObject buttonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/ActionButtonPrefab.prefab");
        if (buttonPrefab == null)
        {
            Debug.LogError("❌ ActionButtonPrefab not found. Run 'RTS/Create Building Selection UI' first.");
            return;
        }

        // Find ButtonContainer
        Transform buttonContainer = uiManager.transform.Find("ButtonContainer");
        if (buttonContainer == null)
        {
            Debug.LogError("❌ ButtonContainer not found under BuildingActionUIManager");
            return;
        }

        // Link references using SerializedObject
        SerializedObject serializedManager = new SerializedObject(uiManager);
        serializedManager.FindProperty("actionButtonPrefab").objectReferenceValue = buttonPrefab;
        serializedManager.FindProperty("buttonContainer").objectReferenceValue = buttonContainer;
        serializedManager.ApplyModifiedProperties();

        EditorUtility.SetDirty(uiManager);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("✅ Linked ActionButtonPrefab and ButtonContainer to BuildingActionUIManager");
    }
}
