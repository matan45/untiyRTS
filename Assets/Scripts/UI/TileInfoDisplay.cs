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
                HexTileSelectionManager.Instance.OnTileSelected += OnTileSelected;
                HexTileSelectionManager.Instance.OnTileDeselected += OnTileDeselected;
            }
            else
            {
                Debug.LogWarning("TileInfoDisplay: HexTileSelectionManager.Instance is null. Will retry in Update.");
            }

            // Hide panel initially
            HidePanel();
        }

        private void Update()
        {
            // Retry subscription if manager wasn't ready at Start
            if (HexTileSelectionManager.Instance != null && _currentTile == null)
            {
                // Check if we need to subscribe
                HexTileSelectionManager.Instance.OnTileSelected -= OnTileSelected;
                HexTileSelectionManager.Instance.OnTileDeselected -= OnTileDeselected;
                HexTileSelectionManager.Instance.OnTileSelected += OnTileSelected;
                HexTileSelectionManager.Instance.OnTileDeselected += OnTileDeselected;
            }
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

            // Update coordinates
            if (coordinatesText != null)
            {
                coordinatesText.text = $"Coordinates: ({tile.Coordinates.x}, {tile.Coordinates.y})";
            }

            // Update terrain type
            if (terrainTypeText != null)
            {
                // Prefer display name from config, fallback to enum name
                string terrainName = tile.TerrainConfig != null
                    ? tile.TerrainConfig.displayName
                    : tile.TerrainType.ToString();
                terrainTypeText.text = $"Terrain: {terrainName}";
            }

            // Update owner
            if (ownerText != null)
            {
                string ownerName = tile.OwnerId < 0 ? "Neutral" : $"Player {tile.OwnerId}";
                ownerText.text = $"Owner: {ownerName}";
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
            _stringBuilder.AppendLine("Resources:");

            bool hasResources = false;
            foreach (ResourceType resourceType in tile.GetResourceTypes())
            {
                int amount = tile.GetResourceAmount(resourceType);
                if (amount > 0)
                {
                    _stringBuilder.AppendLine($"  {resourceType}: {amount}");
                    hasResources = true;
                }
            }

            if (!hasResources)
            {
                _stringBuilder.AppendLine("  None");
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
            _stringBuilder.AppendLine("Modifiers:");

            bool hasModifiers = false;

            // Movement cost (only show if not default)
            if (tile.MovementCost != 1f)
            {
                _stringBuilder.AppendLine($"  Move Cost: {tile.MovementCost:F1}x");
                hasModifiers = true;
            }

            // Defense bonus (only show if not zero)
            if (tile.DefenseBonus != 0)
            {
                string sign = tile.DefenseBonus > 0 ? "+" : "";
                _stringBuilder.AppendLine($"  Defense: {sign}{tile.DefenseBonus}");
                hasModifiers = true;
            }

            // Passable status (only show if not passable)
            if (!tile.IsPassable)
            {
                _stringBuilder.AppendLine("  Impassable");
                hasModifiers = true;
            }

            // Buildable status
            if (!tile.IsBuildable)
            {
                _stringBuilder.AppendLine("  Not Buildable");
                hasModifiers = true;
            }

            if (!hasModifiers)
            {
                _stringBuilder.AppendLine("  None");
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
