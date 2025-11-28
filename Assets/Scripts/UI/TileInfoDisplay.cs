using System.Collections;
using System.Text;
using UnityEngine;
using TMPro;
using RTS.Terrain.Core;
using RTS.Terrain.Selection;
using RTS.Terrain.Data;

namespace RTS.UI
{
    /// <summary>
    /// Displays information about the currently selected hex tile.
    /// Shows: Coordinates, terrain type, ownership, resources, and modifiers.
    /// Implements Single Responsibility Principle (SRP) - only displays tile info.
    /// </summary>
    public class TileInfoDisplay : MonoBehaviour
    {
        [Header("Basic Info")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI coordinatesText;
        [SerializeField] private TextMeshProUGUI terrainTypeText;
        [SerializeField] private TextMeshProUGUI ownerText;

        [Header("Resources Section")]
        [SerializeField] private TextMeshProUGUI resourcesText;

        [Header("Modifiers Section")]
        [SerializeField] private TextMeshProUGUI modifiersText;

        private HexTile _currentTile;
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        private void Awake()
        {
            // Auto-find UI elements if not assigned
            if (panel == null)
                panel = transform.Find("Panel")?.gameObject;

            if (coordinatesText == null)
                coordinatesText = transform.Find("Panel/CoordinatesText")?.GetComponent<TextMeshProUGUI>();

            if (terrainTypeText == null)
                terrainTypeText = transform.Find("Panel/TerrainTypeText")?.GetComponent<TextMeshProUGUI>();

            if (ownerText == null)
                ownerText = transform.Find("Panel/OwnerText")?.GetComponent<TextMeshProUGUI>();

            if (resourcesText == null)
                resourcesText = transform.Find("Panel/ResourcesText")?.GetComponent<TextMeshProUGUI>();

            if (modifiersText == null)
                modifiersText = transform.Find("Panel/ModifiersText")?.GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            // Subscribe to tile selection events
            if (HexTileSelectionManager.Instance != null)
            {
                SubscribeToEvents();
            }
            else
            {
                // Retry subscription using coroutine instead of Update loop
                StartCoroutine(RetrySubscription());
            }

            // Hide panel initially
            HidePanel();
        }

        /// <summary>
        /// Coroutine to retry subscription if manager wasn't ready at Start.
        /// More efficient than checking every frame in Update.
        /// </summary>
        private IEnumerator RetrySubscription()
        {
            while (HexTileSelectionManager.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            SubscribeToEvents();
        }

        /// <summary>
        /// Subscribe to tile selection events.
        /// </summary>
        private void SubscribeToEvents()
        {
            HexTileSelectionManager.Instance.OnTileSelected += OnTileSelected;
            HexTileSelectionManager.Instance.OnTileDeselected += OnTileDeselected;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (HexTileSelectionManager.Instance != null)
            {
                HexTileSelectionManager.Instance.OnTileSelected -= OnTileSelected;
                HexTileSelectionManager.Instance.OnTileDeselected -= OnTileDeselected;
            }
        }

        private void OnTileSelected(HexTile tile)
        {
            if (tile == null)
            {
                HidePanel();
                return;
            }

            _currentTile = tile;
            UpdateDisplay(tile);
            ShowPanel();
        }

        private void OnTileDeselected(HexTile tile)
        {
            _currentTile = null;
            HidePanel();
        }

        /// <summary>
        /// Updates the display with information from the specified tile.
        /// </summary>
        public void UpdateDisplay(HexTile tile)
        {
            if (tile == null)
            {
                ClearDisplay();
                return;
            }

            // Update coordinates with bold label
            if (coordinatesText != null)
            {
                coordinatesText.text = $"<b>Position:</b> ({tile.Coordinates.x}, {tile.Coordinates.y})";
            }

            // Update terrain type with bold label
            if (terrainTypeText != null)
            {
                // Prefer display name from config, fallback to enum name
                string terrainName = tile.TerrainConfig != null
                    ? tile.TerrainConfig.displayName
                    : tile.TerrainType.ToString();
                terrainTypeText.text = $"<b>Terrain:</b> {terrainName}";
            }

            // Update owner with bold label
            if (ownerText != null)
            {
                string ownerName = tile.OwnerId < 0 ? "Neutral" : $"Player {tile.OwnerId}";
                ownerText.text = $"<b>Owner:</b> {ownerName}";
            }

            // Update resources
            UpdateResourcesDisplay(tile);

            // Update modifiers
            UpdateModifiersDisplay(tile);
        }

        /// <summary>
        /// Updates the resources display section.
        /// </summary>
        private void UpdateResourcesDisplay(HexTile tile)
        {
            if (resourcesText == null) return;

            _stringBuilder.Clear();
            _stringBuilder.AppendLine("<b>Resources:</b>");

            bool hasResources = false;
            // Use Resources property for non-allocating iteration
            foreach (var kvp in tile.Resources)
            {
                if (kvp.Value.currentAmount > 0)
                {
                    _stringBuilder.AppendLine($"  <color=#FFFFFF>{kvp.Key}:</color> {kvp.Value.currentAmount}");
                    hasResources = true;
                }
            }

            if (!hasResources)
            {
                _stringBuilder.AppendLine("  <color=#888888>None</color>");
            }

            resourcesText.text = _stringBuilder.ToString().TrimEnd();
        }

        /// <summary>
        /// Updates the modifiers display section.
        /// </summary>
        private void UpdateModifiersDisplay(HexTile tile)
        {
            if (modifiersText == null) return;

            _stringBuilder.Clear();
            _stringBuilder.AppendLine("<b>Modifiers:</b>");

            bool hasModifiers = false;

            // Movement cost (only show if not default)
            if (tile.MovementCost != 1f)
            {
                _stringBuilder.AppendLine($"  <color=#FFFFFF>Move Cost:</color> {tile.MovementCost:F1}x");
                hasModifiers = true;
            }

            // Defense bonus (only show if not zero)
            if (tile.DefenseBonus != 0)
            {
                string sign = tile.DefenseBonus > 0 ? "+" : "";
                string bonusColor = tile.DefenseBonus > 0 ? "#88CC88" : "#CC8888"; // Green for positive, red for negative
                _stringBuilder.AppendLine($"  <color=#FFFFFF>Defense:</color> <color={bonusColor}>{sign}{tile.DefenseBonus}</color>");
                hasModifiers = true;
            }

            // Passable status (only show if not passable)
            if (!tile.IsPassable)
            {
                _stringBuilder.AppendLine("  <color=#CC6666>Impassable</color>");
                hasModifiers = true;
            }

            // Buildable status
            if (!tile.IsBuildable)
            {
                _stringBuilder.AppendLine("  <color=#CC9966>Not Buildable</color>");
                hasModifiers = true;
            }

            if (!hasModifiers)
            {
                _stringBuilder.AppendLine("  <color=#888888>None</color>");
            }

            modifiersText.text = _stringBuilder.ToString().TrimEnd();
        }

        /// <summary>
        /// Clears all displayed information.
        /// </summary>
        public void ClearDisplay()
        {
            _currentTile = null;

            if (coordinatesText != null)
                coordinatesText.text = "";

            if (terrainTypeText != null)
                terrainTypeText.text = "";

            if (ownerText != null)
                ownerText.text = "";

            if (resourcesText != null)
                resourcesText.text = "";

            if (modifiersText != null)
                modifiersText.text = "";
        }

        private void ShowPanel()
        {
            if (panel != null)
                panel.SetActive(true);
        }

        private void HidePanel()
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }
}
