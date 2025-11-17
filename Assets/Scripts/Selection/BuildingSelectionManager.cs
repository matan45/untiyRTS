using System;
using System.Collections.Generic;
using RTS.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RTS.Selection
{
    /// <summary>
    /// Singleton manager handling building selection via mouse clicks.
    /// Implements Observer Pattern - notifies listeners via events when selection changes.
    /// Implements Single Responsibility Principle (SRP) - only manages selection state.
    /// </summary>
    public class BuildingSelectionManager : MonoBehaviour
    {
        public static BuildingSelectionManager Instance { get; private set; }

        // Static flag to prevent repeated EventSystem warnings
        private static bool hasWarnedAboutEventSystem = false;

        // Flag to temporarily block selection after building placement (per CLAUDE.md: event-driven architecture)
        private bool blockSelectionThisFrame = false;

        [Header("Selection Settings")]
        [Tooltip("Layer mask for selectable objects")]
        [SerializeField] private LayerMask selectableLayer = ~0; // All layers by default

        [Tooltip("Maximum raycast distance for selection")]
        [SerializeField] private float maxRaycastDistance = 1000f;

        [Header("Input Actions")]
        [Tooltip("Reference to the RTS Input Actions asset")]
        [SerializeField] private InputActionAsset inputActions;

        // Events - Observer Pattern for decoupled communication
        public event Action<ISelectable> OnSelectionChanged;
        public event Action OnSelectionCleared;

        private ISelectable currentSelection;
        private Camera mainCamera;
        private InputAction leftClickAction;
        private InputAction mousePositionAction;
        private InputAction rightClickAction;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogError("BuildingSelectionManager: No main camera found!");
                enabled = false;
                return;
            }

            SetupInputActions();

            // Subscribe to building placement event (per CLAUDE.md: event-driven architecture)
            if (BuildingPlacer.Instance != null)
            {
                BuildingPlacer.Instance.OnBuildingPlaced += OnBuildingPlaced;
            }
        }

        private void OnBuildingPlaced()
        {
            // Block selection for one frame after placement to prevent immediate selection
            blockSelectionThisFrame = true;
            StartCoroutine(UnblockSelectionNextFrame());
        }

        private System.Collections.IEnumerator UnblockSelectionNextFrame()
        {
            yield return null; // Wait one frame
            blockSelectionThisFrame = false;
        }

        private void SetupInputActions()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("BuildingSelectionManager: No InputActionAsset assigned. Trying to find RTSInputActions...");
                inputActions = Resources.Load<InputActionAsset>("RTSInputActions");

                if (inputActions == null)
                {
                    Debug.LogError("BuildingSelectionManager: Could not find RTSInputActions!");
                    return;
                }
            }

            // Get the Camera action map
            var cameraMap = inputActions.FindActionMap("Camera");
            if (cameraMap != null)
            {
                leftClickAction = cameraMap.FindAction("LeftClick");
                mousePositionAction = cameraMap.FindAction("MousePosition");
                rightClickAction = cameraMap.FindAction("RightClick");

                if (leftClickAction != null)
                {
                    leftClickAction.performed += OnLeftClick;
                }

                if (rightClickAction != null)
                {
                    rightClickAction.performed += OnRightClick;
                }
            }
        }

        private void OnEnable()
        {
            leftClickAction?.Enable();
            rightClickAction?.Enable();
            mousePositionAction?.Enable();
        }

        private void OnDisable()
        {
            leftClickAction?.Disable();
            rightClickAction?.Disable();
            mousePositionAction?.Disable();
        }

        private void OnDestroy()
        {
            if (leftClickAction != null)
            {
                leftClickAction.performed -= OnLeftClick;
            }

            if (rightClickAction != null)
            {
                rightClickAction.performed -= OnRightClick;
            }

            // Unsubscribe from building placement event
            if (BuildingPlacer.Instance != null)
            {
                BuildingPlacer.Instance.OnBuildingPlaced -= OnBuildingPlaced;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnLeftClick(InputAction.CallbackContext context)
        {
            // Don't select if currently placing a building (per CLAUDE.md: event-driven architecture)
            if (BuildingPlacer.Instance != null && BuildingPlacer.Instance.IsPlacing())
            {
                return;
            }

            // Don't select if a building was just placed this frame
            if (blockSelectionThisFrame)
            {
                return;
            }

            // Don't select if mouse is over UI (use proper check for new Input System)
            if (IsPointerOverUI())
            {
                return;
            }

            TrySelectAtMousePosition();
        }

        private bool IsPointerOverUI()
        {
            // Per CLAUDE.md: Handle edge cases - missing EventSystem should prevent world clicks
            if (EventSystem.current == null)
            {
                if (!hasWarnedAboutEventSystem)
                {
                    Debug.LogError("BuildingSelectionManager: EventSystem.current is null! UI input detection will not work. " +
                                   "Make sure an EventSystem exists in the scene.");
                    hasWarnedAboutEventSystem = true;
                }
                return true; // Return true to block world clicks when EventSystem is missing
            }

            if (mousePositionAction == null)
                return false;

            Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = mousePos
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Count > 0;
        }

        private void OnRightClick(InputAction.CallbackContext context)
        {
            // Right-click clears selection
            ClearSelection();
        }

        private void TrySelectAtMousePosition()
        {
            if (mousePositionAction == null || mainCamera == null)
                return;

            Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, selectableLayer))
            {
                // Try to get ISelectable from the hit object
                var selectable = hit.collider.GetComponent<ISelectable>();

                if (selectable != null)
                {
                    SelectObject(selectable);
                }
                else
                {
                    // Clicked on something that's not selectable - clear selection
                    ClearSelection();
                }
            }
            else
            {
                // Clicked on empty space - clear selection
                ClearSelection();
            }
        }

        /// <summary>
        /// Selects the specified object.
        /// </summary>
        /// <param name="selectable">The object to select</param>
        public void SelectObject(ISelectable selectable)
        {
            if (selectable == null)
            {
                ClearSelection();
                return;
            }

            // Don't re-select the same object
            if (currentSelection == selectable)
                return;

            // Deselect previous selection
            if (currentSelection != null)
            {
                currentSelection.Deselect();
            }

            // Select new object
            currentSelection = selectable;
            currentSelection.Select();

            // Notify listeners
            OnSelectionChanged?.Invoke(currentSelection);
        }

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        public void ClearSelection()
        {
            if (currentSelection == null)
                return;

            currentSelection.Deselect();
            currentSelection = null;

            // Notify listeners
            OnSelectionCleared?.Invoke();
        }

        /// <summary>
        /// Gets the currently selected object.
        /// </summary>
        /// <returns>The current selection, or null if nothing is selected</returns>
        public ISelectable GetCurrentSelection()
        {
            return currentSelection;
        }

        /// <summary>
        /// Checks if there is an active selection.
        /// </summary>
        public bool HasSelection()
        {
            return currentSelection != null;
        }
    }
}
