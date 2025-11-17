using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using RTS.UI;

/// <summary>
/// Editor script to automatically create and configure the Building Action UI hierarchy
/// </summary>
public class SetupBuildingActionUI : EditorWindow
{
    private Canvas targetCanvas;
    private GameObject actionButtonPrefab;

    [MenuItem("RTS/Setup Building Action UI")]
    public static void ShowWindow()
    {
        GetWindow<SetupBuildingActionUI>("Setup Building Action UI");
    }

    private void OnGUI()
    {
        GUILayout.Label("Building Action UI Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        targetCanvas = (Canvas)EditorGUILayout.ObjectField("Target Canvas", targetCanvas, typeof(Canvas), true);
        actionButtonPrefab = (GameObject)EditorGUILayout.ObjectField("Action Button Prefab", actionButtonPrefab, typeof(GameObject), false);

        GUILayout.Space(10);

        if (GUILayout.Button("Create Complete UI Hierarchy", GUILayout.Height(40)))
        {
            CreateCompleteUIHierarchy();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Fix Existing UI Setup", GUILayout.Height(30)))
        {
            FixExistingSetup();
        }
    }

    private void CreateCompleteUIHierarchy()
    {
        if (targetCanvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a target Canvas!", "OK");
            return;
        }

        // Create main action panel
        GameObject actionPanel = CreateActionPanel(targetCanvas.transform);

        // Create background
        GameObject background = CreateBackground(actionPanel.transform);

        // Create header/info section
        GameObject infoSection = CreateInfoSection(actionPanel.transform);

        // Create button container
        GameObject buttonContainer = CreateButtonContainer(actionPanel.transform);

        // Create accent border (optional decorative element)
        GameObject accentBorder = CreateAccentBorder(actionPanel.transform);

        // Find or create BuildingActionUIManager
        BuildingActionUIManager uiManager = FindObjectOfType<BuildingActionUIManager>();
        if (uiManager == null)
        {
            uiManager = actionPanel.AddComponent<BuildingActionUIManager>();
            Debug.Log("Created new BuildingActionUIManager component");
        }

        // Wire up references
        WireUpReferences(uiManager, actionPanel, background, infoSection, buttonContainer, accentBorder);

        // Mark scene dirty
        EditorUtility.SetDirty(actionPanel);
        EditorUtility.SetDirty(uiManager);

        Debug.Log("✅ Building Action UI hierarchy created successfully!");
        EditorUtility.DisplayDialog("Success", "Building Action UI hierarchy created successfully!\n\nCheck the scene for 'BuildingActionPanel'.", "OK");

        // Select the created panel
        Selection.activeGameObject = actionPanel;
    }

    private GameObject CreateActionPanel(Transform parent)
    {
        GameObject panel = new GameObject("BuildingActionPanel");
        panel.transform.SetParent(parent, false);

        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = new Vector2(0, 50);  // Moved higher from bottom
        rectTransform.sizeDelta = new Vector2(700, 400);  // Increased width and height

        CanvasGroup canvasGroup = panel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        return panel;
    }

    private GameObject CreateBackground(Transform parent)
    {
        GameObject background = new GameObject("Background");
        background.transform.SetParent(parent, false);

        RectTransform rectTransform = background.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        Image image = background.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        image.raycastTarget = false;

        return background;
    }

    private GameObject CreateInfoSection(Transform parent)
    {
        GameObject infoSection = new GameObject("InfoSection");
        infoSection.transform.SetParent(parent, false);

        RectTransform rectTransform = infoSection.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.anchoredPosition = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(0, 60);

        // Header background
        GameObject headerBg = new GameObject("HeaderBackground");
        headerBg.transform.SetParent(infoSection.transform, false);

        RectTransform headerRect = headerBg.AddComponent<RectTransform>();
        headerRect.anchorMin = Vector2.zero;
        headerRect.anchorMax = Vector2.one;
        headerRect.sizeDelta = Vector2.zero;

        Image headerImage = headerBg.AddComponent<Image>();
        headerImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        headerImage.raycastTarget = false;

        // Building name text
        GameObject nameText = new GameObject("BuildingNameText");
        nameText.transform.SetParent(infoSection.transform, false);

        RectTransform nameRect = nameText.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(1, 0.5f);
        nameRect.pivot = new Vector2(0.5f, 0.5f);
        nameRect.anchoredPosition = Vector2.zero;
        nameRect.sizeDelta = new Vector2(-20, 40);

        TextMeshProUGUI nameTextComponent = nameText.AddComponent<TextMeshProUGUI>();
        nameTextComponent.text = "Building Name";
        nameTextComponent.fontSize = 24;
        nameTextComponent.alignment = TextAlignmentOptions.Center;
        nameTextComponent.color = Color.white;
        nameTextComponent.raycastTarget = false;

        // Add BuildingInfoDisplay component
        BuildingInfoDisplay infoDisplay = infoSection.AddComponent<BuildingInfoDisplay>();

        return infoSection;
    }

    private GameObject CreateButtonContainer(Transform parent)
    {
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonContainer.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0, -35);  // Centered better
        rectTransform.sizeDelta = new Vector2(-40, -100);  // More padding from edges

        // Add default GridLayoutGroup (will be replaced dynamically)
        GridLayoutGroup gridLayout = buttonContainer.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(120, 120);
        gridLayout.spacing = new Vector2(10, 10);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 3;
        gridLayout.childAlignment = TextAnchor.MiddleCenter; // Center buttons in the container

        return buttonContainer;
    }

    private GameObject CreateAccentBorder(Transform parent)
    {
        GameObject accentBorder = new GameObject("AccentBorder");
        accentBorder.transform.SetParent(parent, false);

        RectTransform rectTransform = accentBorder.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        Image image = accentBorder.AddComponent<Image>();
        image.color = new Color(0, 1, 0, 0.3f);
        image.raycastTarget = false;

        // Use Outline component for border effect
        Outline outline = accentBorder.AddComponent<Outline>();
        outline.effectColor = new Color(0, 1, 0, 1f);
        outline.effectDistance = new Vector2(2, -2);

        return accentBorder;
    }

    private void WireUpReferences(BuildingActionUIManager uiManager, GameObject actionPanel,
        GameObject background, GameObject infoSection, GameObject buttonContainer, GameObject accentBorder)
    {
        SerializedObject so = new SerializedObject(uiManager);

        // Wire up main references
        so.FindProperty("actionPanel").objectReferenceValue = actionPanel;
        so.FindProperty("buttonContainer").objectReferenceValue = buttonContainer.transform;

        if (actionButtonPrefab != null)
        {
            so.FindProperty("actionButtonPrefab").objectReferenceValue = actionButtonPrefab;
        }

        // Wire up info display
        BuildingInfoDisplay infoDisplay = infoSection.GetComponent<BuildingInfoDisplay>();
        if (infoDisplay != null)
        {
            so.FindProperty("infoDisplay").objectReferenceValue = infoDisplay;
        }

        // Wire up customization references
        Image bgImage = background.GetComponent<Image>();
        if (bgImage != null)
        {
            so.FindProperty("panelBackgroundImage").objectReferenceValue = bgImage;
        }

        Image headerImage = infoSection.transform.Find("HeaderBackground").GetComponent<Image>();
        if (headerImage != null)
        {
            so.FindProperty("headerBackgroundImage").objectReferenceValue = headerImage;
        }

        Image accentImage = accentBorder.GetComponent<Image>();
        if (accentImage != null)
        {
            so.FindProperty("accentBorderImage").objectReferenceValue = accentImage;
        }

        so.ApplyModifiedProperties();

        Debug.Log("✅ All references wired up to BuildingActionUIManager");
    }

    private void FixExistingSetup()
    {
        BuildingActionUIManager uiManager = FindObjectOfType<BuildingActionUIManager>();

        if (uiManager == null)
        {
            EditorUtility.DisplayDialog("Error", "No BuildingActionUIManager found in scene!\n\nPlease create a complete UI hierarchy first.", "OK");
            return;
        }

        SerializedObject so = new SerializedObject(uiManager);

        // Try to find and wire up missing references
        GameObject actionPanel = uiManager.gameObject;

        // Find background
        Transform bgTransform = actionPanel.transform.Find("Background");
        if (bgTransform != null)
        {
            Image bgImage = bgTransform.GetComponent<Image>();
            if (bgImage != null)
            {
                so.FindProperty("panelBackgroundImage").objectReferenceValue = bgImage;
                Debug.Log("✅ Wired up Background Image");
            }
        }

        // Find info section
        Transform infoTransform = actionPanel.transform.Find("InfoSection");
        if (infoTransform != null)
        {
            BuildingInfoDisplay infoDisplay = infoTransform.GetComponent<BuildingInfoDisplay>();
            if (infoDisplay != null)
            {
                so.FindProperty("infoDisplay").objectReferenceValue = infoDisplay;
                Debug.Log("✅ Wired up BuildingInfoDisplay");
            }

            Transform headerTransform = infoTransform.Find("HeaderBackground");
            if (headerTransform != null)
            {
                Image headerImage = headerTransform.GetComponent<Image>();
                if (headerImage != null)
                {
                    so.FindProperty("headerBackgroundImage").objectReferenceValue = headerImage;
                    Debug.Log("✅ Wired up Header Background Image");
                }
            }
        }

        // Find button container
        Transform buttonTransform = actionPanel.transform.Find("ButtonContainer");
        if (buttonTransform != null)
        {
            so.FindProperty("buttonContainer").objectReferenceValue = buttonTransform;
            Debug.Log("✅ Wired up Button Container");
        }

        // Find accent border
        Transform accentTransform = actionPanel.transform.Find("AccentBorder");
        if (accentTransform != null)
        {
            Image accentImage = accentTransform.GetComponent<Image>();
            if (accentImage != null)
            {
                so.FindProperty("accentBorderImage").objectReferenceValue = accentImage;
                Debug.Log("✅ Wired up Accent Border Image");
            }
        }

        if (actionButtonPrefab != null)
        {
            so.FindProperty("actionButtonPrefab").objectReferenceValue = actionButtonPrefab;
            Debug.Log("✅ Assigned Action Button Prefab");
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(uiManager);

        Debug.Log("✅ Fixed existing UI setup!");
        EditorUtility.DisplayDialog("Success", "Existing UI setup fixed!\n\nCheck console for details.", "OK");
    }
}
