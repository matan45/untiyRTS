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
        // Color palette for UI
        private static readonly Color PanelBackground = new Color(0.12f, 0.12f, 0.15f, 0.95f);
        private static readonly Color HeaderColor = new Color(0.95f, 0.85f, 0.45f, 1f); // Gold
        private static readonly Color LabelColor = new Color(0.7f, 0.75f, 0.8f, 1f); // Light blue-gray
        private static readonly Color SectionHeaderColor = new Color(0.5f, 0.8f, 0.6f, 1f); // Soft green
        private static readonly Color BorderColor = new Color(0.3f, 0.35f, 0.4f, 1f);

        private static TMP_FontAsset GetFont()
        {
            Debug.Log("GetFont() called - searching for fonts...");

            // Direct path to Roboto font
            var robotoFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/Roboto SDF.asset");
            Debug.Log($"Roboto font result: {(robotoFont != null ? robotoFont.name : "NULL")}");

            if (robotoFont != null)
            {
                return robotoFont;
            }

            // Fallback to Liberation Sans
            var liberationFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            Debug.Log($"LiberationSans font result: {(liberationFont != null ? liberationFont.name : "NULL")}");

            if (liberationFont != null)
            {
                return liberationFont;
            }

            // Fallback to TMP default
            Debug.Log("Using default TMP font");
            return TMP_Settings.defaultFontAsset;
        }

        [MenuItem("Window/RTS/Create Tile Info Panel")]
        public static void CreateTileInfoPanel()
        {
            Debug.Log("=== CreateTileInfoPanel STARTED ===");

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
            panelRect.sizeDelta = new Vector2(240, 290);

            // Create Panel background with border effect
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(tileInfoPanel.transform, false);

            RectTransform bgRect = panel.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = panel.AddComponent<Image>();
            bgImage.color = PanelBackground;

            // Add outline for border effect
            var outline = panel.AddComponent<Outline>();
            outline.effectColor = BorderColor;
            outline.effectDistance = new Vector2(2, 2);

            // Add Vertical Layout Group
            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(15, 15, 12, 12);
            layout.spacing = 6;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            // Create Title with Bold uppercase styling - larger for emphasis
            CreateStyledText(panel.transform, "TitleText", "TILE INFO", 20, FontStyles.Bold, HeaderColor, 30, true, 4f);

            // Add separator line
            CreateSeparator(panel.transform);

            // Create Coordinates text - clean sans-serif style
            CreateStyledText(panel.transform, "CoordinatesText", "<b>Position:</b> (0, 0)", 14, FontStyles.Normal, LabelColor, 24);

            // Create Terrain Type text
            CreateStyledText(panel.transform, "TerrainTypeText", "<b>Terrain:</b> Grassland", 14, FontStyles.Normal, LabelColor, 24);

            // Create Owner text
            CreateStyledText(panel.transform, "OwnerText", "<b>Owner:</b> Neutral", 14, FontStyles.Normal, LabelColor, 24);

            // Add separator before sections
            CreateSeparator(panel.transform);

            // Create Resources text (multi-line)
            CreateStyledText(panel.transform, "ResourcesText", "<b>Resources:</b>\n  None", 13, FontStyles.Normal, SectionHeaderColor, 60);

            // Create Modifiers text (multi-line)
            CreateStyledText(panel.transform, "ModifiersText", "<b>Modifiers:</b>\n  None", 13, FontStyles.Normal, SectionHeaderColor, 60);

            // Add TileInfoDisplay component
            var displayComponent = tileInfoPanel.AddComponent<RTS.UI.TileInfoDisplay>();

            // Wire up references using SerializedObject
            SerializedObject so = new SerializedObject(displayComponent);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("coordinatesText").objectReferenceValue = panel.transform.Find("CoordinatesText").GetComponent<TextMeshProUGUI>();
            so.FindProperty("terrainTypeText").objectReferenceValue = panel.transform.Find("TerrainTypeText").GetComponent<TextMeshProUGUI>();
            so.FindProperty("ownerText").objectReferenceValue = panel.transform.Find("OwnerText").GetComponent<TextMeshProUGUI>();
            so.FindProperty("resourcesText").objectReferenceValue = panel.transform.Find("ResourcesText").GetComponent<TextMeshProUGUI>();
            so.FindProperty("modifiersText").objectReferenceValue = panel.transform.Find("ModifiersText").GetComponent<TextMeshProUGUI>();
            so.ApplyModifiedProperties();

            // Select the created object
            UnityEditor.Selection.activeGameObject = tileInfoPanel;

            Debug.Log("TileInfoPanel created successfully! Panel will appear at bottom-left of screen.");
        }

        private static void CreateStyledText(Transform parent, string name, string defaultText, int fontSize, FontStyles style, Color color, float height, bool addShadow = false, float charSpacing = 0.5f)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(190, height);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();

            // Apply font
            var font = GetFont();
            if (font != null)
            {
                tmp.font = font;
            }

            tmp.text = defaultText;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Truncate;
            tmp.richText = true; // Enable rich text for inline styling
            tmp.characterSpacing = charSpacing;
            tmp.lineSpacing = 2f;
            tmp.fontWeight = FontWeight.Regular;

            // Improve text rendering quality
            tmp.extraPadding = true;

            // Add shadow effect for headers
            if (addShadow)
            {
                var shadow = textObj.AddComponent<Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.5f);
                shadow.effectDistance = new Vector2(1, -1);
            }
        }

        private static void CreateSeparator(Transform parent)
        {
            GameObject separator = new GameObject("Separator");
            separator.transform.SetParent(parent, false);

            RectTransform sepRect = separator.AddComponent<RectTransform>();
            sepRect.sizeDelta = new Vector2(190, 2);

            Image sepImage = separator.AddComponent<Image>();
            sepImage.color = new Color(0.4f, 0.45f, 0.5f, 0.5f);
        }
    }
}
