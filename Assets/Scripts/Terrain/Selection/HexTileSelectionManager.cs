using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using RTS.Terrain.Core;
using RTS.Terrain.Rendering;

namespace RTS.Terrain.Selection
{
    /// <summary>
    /// Manages hex tile selection via raycasting.
    /// Separate from building selection (ISP - different concerns).
    /// </summary>
    public class HexTileSelectionManager : MonoBehaviour
    {
        public static HexTileSelectionManager Instance { get; private set; }

        [Header("Raycast Settings")]
        [SerializeField, Tooltip("Camera used for raycasting (defaults to main camera)")]
        private Camera raycastCamera;

        [SerializeField, Tooltip("Layer mask for hex tiles")]
        private LayerMask hexTileLayerMask;

        [SerializeField, Tooltip("Maximum raycast distance")]
        private float maxRaycastDistance = 1000f;

        [Header("Input")]
        [SerializeField, Tooltip("Input action for tile selection")]
        private InputActionReference selectAction;

        [Header("Behavior")]
        [SerializeField, Tooltip("Enable hover detection")]
        private bool enableHover = true;

        [SerializeField, Tooltip("Clear selection on right-click")]
        private bool clearOnRightClick = true;

        /// <summary>
        /// Event fired when a tile is selected.
        /// </summary>
        public event Action<HexTile> OnTileSelected;

        /// <summary>
        /// Event fired when selection is cleared.
        /// </summary>
        public event Action<HexTile> OnTileDeselected;

        /// <summary>
        /// Event fired when hovering over a tile.
        /// </summary>
        public event Action<HexTile> OnTileHovered;

        /// <summary>
        /// Event fired when hover leaves a tile.
        /// </summary>
        public event Action OnTileHoverExit;

        private HexTile _selectedTile;
        private HexTile _hoveredTile;
        private bool _isSelectionBlocked;
        private bool _selectPending;

        /// <summary>
        /// Currently selected tile.
        /// </summary>
        public HexTile SelectedTile => _selectedTile;

        /// <summary>
        /// Currently hovered tile.
        /// </summary>
        public HexTile HoveredTile => _hoveredTile;

        /// <summary>
        /// Whether tile selection is currently blocked.
        /// </summary>
        public bool IsSelectionBlocked => _isSelectionBlocked;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"HexTileSelectionManager: Duplicate instance on {gameObject.name}, destroying.");
                Destroy(this);
                return;
            }

            Instance = this;

            if (raycastCamera == null)
            {
                raycastCamera = Camera.main;
            }

            // Set up layer mask if not configured
            if (hexTileLayerMask == 0)
            {
                int layer = LayerMask.NameToLayer("HexTile");
                if (layer >= 0)
                {
                    hexTileLayerMask = 1 << layer;
                    Debug.Log($"HexTileSelectionManager: Auto-configured layer mask for 'HexTile' (layer {layer})");
                }
                else
                {
                    // Fallback to Default layer
                    hexTileLayerMask = 1; // Layer 0 (Default)
                    Debug.LogWarning("HexTileSelectionManager: 'HexTile' layer not found! Using Default layer. Selection may not work correctly.");
                }
            }
        }

        private void OnEnable()
        {
            if (selectAction != null && selectAction.action != null)
            {
                selectAction.action.Enable();
                selectAction.action.performed += OnSelectPerformed;
            }
        }

        private void OnDisable()
        {
            if (selectAction != null && selectAction.action != null)
            {
                selectAction.action.performed -= OnSelectPerformed;
                selectAction.action.Disable();
            }
        }

        private void Update()
        {
            if (enableHover && !_isSelectionBlocked)
            {
                UpdateHover();
            }

            // Process pending selection (deferred from InputAction callback)
            if (_selectPending)
            {
                _selectPending = false;
                TrySelectTileUnderMouse();
            }

            // Handle left-click for selection (fallback if no InputAction assigned)
            if (selectAction == null || selectAction.action == null)
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    TrySelectTileUnderMouse();
                }
            }

            // Handle right-click to clear selection
            if (clearOnRightClick && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                ClearSelection();
            }
        }

        private void OnSelectPerformed(InputAction.CallbackContext context)
        {
            if (_isSelectionBlocked) return;

            // Defer selection to Update to avoid IsPointerOverGameObject() issues
            _selectPending = true;
        }

        /// <summary>
        /// Try to select the tile currently under the mouse cursor.
        /// </summary>
        public void TrySelectTileUnderMouse()
        {
            if (_isSelectionBlocked) return;
            if (IsPointerOverUI()) return;

            HexTile tile = GetTileUnderMouse();
            if (tile != null)
            {
                SelectTile(tile);
            }
        }

        /// <summary>
        /// Check if the mouse pointer is over a UI element.
        /// </summary>
        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Update hover state based on mouse position.
        /// </summary>
        private void UpdateHover()
        {
            HexTile tile = GetTileUnderMouse();

            if (tile != _hoveredTile)
            {
                if (_hoveredTile != null)
                {
                    OnTileHoverExit?.Invoke();
                }

                _hoveredTile = tile;

                if (_hoveredTile != null)
                {
                    OnTileHovered?.Invoke(_hoveredTile);
                }
            }
        }

        /// <summary>
        /// Get the hex tile under the current mouse position.
        /// </summary>
        public HexTile GetTileUnderMouse()
        {
            if (raycastCamera == null) return null;

            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            Ray ray = raycastCamera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, hexTileLayerMask))
            {
                var tileObj = hit.collider.GetComponent<HexTileObject>();
                if (tileObj != null)
                {
                    return tileObj.TileData;
                }
            }

            return null;
        }

        /// <summary>
        /// Select a specific tile.
        /// </summary>
        public void SelectTile(HexTile tile)
        {
            if (tile == null) return;

            // Deselect previous if different
            if (_selectedTile != null && _selectedTile != tile)
            {
                HexTile previousTile = _selectedTile;
                _selectedTile = null;
                OnTileDeselected?.Invoke(previousTile);
            }

            // Select new tile
            if (_selectedTile != tile)
            {
                _selectedTile = tile;
                Debug.Log($"HexTileSelectionManager: Selected tile at {tile.Coordinates} ({tile.TerrainType})");
                OnTileSelected?.Invoke(_selectedTile);
            }
        }

        /// <summary>
        /// Clear the current selection.
        /// </summary>
        public void ClearSelection()
        {
            if (_selectedTile != null)
            {
                HexTile previousTile = _selectedTile;
                _selectedTile = null;
                OnTileDeselected?.Invoke(previousTile);
            }
        }

        /// <summary>
        /// Block tile selection (e.g., during building placement).
        /// </summary>
        public void BlockSelection(bool blocked)
        {
            _isSelectionBlocked = blocked;

            if (blocked)
            {
                // Clear hover when blocked
                if (_hoveredTile != null)
                {
                    _hoveredTile = null;
                    OnTileHoverExit?.Invoke();
                }
            }
        }

        /// <summary>
        /// Check if a tile is the currently selected tile.
        /// </summary>
        public bool IsSelected(HexTile tile)
        {
            return tile != null && _selectedTile == tile;
        }

        /// <summary>
        /// Check if a tile is the currently hovered tile.
        /// </summary>
        public bool IsHovered(HexTile tile)
        {
            return tile != null && _hoveredTile == tile;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
