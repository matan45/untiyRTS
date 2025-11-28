using UnityEngine;
using TMPro;
using RTS.Terrain.Core;
using RTS.Terrain.Selection;

namespace RTS.UI
{
    /// <summary>
    /// Displays information about the currently selected hex tile.
    /// Shows: Coordinates, terrain type, and ownership.
    /// Implements Single Responsibility Principle (SRP) - only displays tile info.
    /// </summary>
    public class TileInfoDisplay : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI coordinatesText;
        [SerializeField] private TextMeshProUGUI terrainTypeText;
        [SerializeField] private TextMeshProUGUI ownerText;

        private HexTile _currentTile;

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
