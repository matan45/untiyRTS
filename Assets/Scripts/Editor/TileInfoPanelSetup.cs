using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace RTS.Editor
{
    /// <summary>
    /// Editor utility to create the TileInfoPanel UI structure.
    /// Use: Window > RTS > Create Tile Info Panel
    /// </summary>
    public class TileInfoPanelSetup
    {
        [MenuItem("Window/RTS/Create Tile Info Panel")]
        public static void CreateTileInfoPanel()
        {
            // Find existing Canvas (BuildingUI)
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in scene. Please create a Canvas first.");
                return;
            }

            // Create main panel container
            GameObject tileInfoPanel = new GameObject("TileInfoPanel");
            tileInfoPanel.transform.SetParent(canvas.transform, false);

            // Add RectTransform and position at bottom-left
            RectTransform panelRect = tileInfoPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(0, 0);
            panelRect.pivot = new Vector2(0, 0);
            panelRect.anchoredPosition = new Vector2(20, 20);
            panelRect.sizeDelta = new Vector2(200, 120);

            // Create Panel background
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(tileInfoPanel.transform, false);

            RectTransform bgRect = panel.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = panel.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

            // Add Vertical Layout Group
            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            // Create Title text
            CreateTextElement(panel.transform, "TitleText", "Tile Info", 16, FontStyles.Bold);

            // Create Coordinates text
            CreateTextElement(panel.transform, "CoordinatesText", "Coordinates: (0, 0)", 14, FontStyles.Normal);

            // Create Terrain Type text
            CreateTextElement(panel.transform, "TerrainTypeText", "Terrain: Grassland", 14, FontStyles.Normal);

            // Create Owner text
            CreateTextElement(panel.transform, "OwnerText", "Owner: Neutral", 14, FontStyles.Normal);

            // Add TileInfoDisplay component
            var displayComponent = tileInfoPanel.AddComponent<RTS.UI.TileInfoDisplay>();

            // Wire up references using SerializedObject
            SerializedObject so = new SerializedObject(displayComponent);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("coordinatesText").objectReferenceValue = panel.transform.Find("CoordinatesText").GetComponent<TextMeshProUGUI>();
            so.FindProperty("terrainTypeText").objectReferenceValue = panel.transform.Find("TerrainTypeText").GetComponent<TextMeshProUGUI>();
            so.FindProperty("ownerText").objectReferenceValue = panel.transform.Find("OwnerText").GetComponent<TextMeshProUGUI>();
            so.ApplyModifiedProperties();

            // Select the created object
            UnityEditor.Selection.activeGameObject = tileInfoPanel;

            Debug.Log("TileInfoPanel created successfully! Panel will appear at bottom-left of screen.");
        }

        private static void CreateTextElement(Transform parent, string name, string defaultText, int fontSize, FontStyles style)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(180, 20);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
        }
    }
}
