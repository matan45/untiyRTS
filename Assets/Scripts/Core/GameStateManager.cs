using System;
using UnityEngine;
using RTS.Terrain.Core;

namespace RTS.Core
{
    /// <summary>
    /// Central game state manager.
    /// Coordinates map initialization and provides hooks for turn-based updates.
    /// NOTE: This is a SCENE-SPECIFIC singleton - state resets per level.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [Header("Singleton Settings")]
        [SerializeField, Tooltip("If true, game state will persist across scene loads. Typically FALSE for level-based games.")]
        private bool persistAcrossScenes = false;

        #region Map State

        /// <summary>
        /// Whether the map is fully loaded and ready for use.
        /// </summary>
        public bool IsMapReady { get; private set; }

        /// <summary>
        /// Event fired when the map becomes ready.
        /// Subscribe to this to initialize systems that depend on map data.
        /// </summary>
        public event Action OnMapReady;

        /// <summary>
        /// API facade for accessing map/tile information.
        /// Only available after IsMapReady is true.
        /// </summary>
        public MapService Map { get; private set; }

        #endregion

        #region Game Phase

        /// <summary>
        /// Current phase of the game.
        /// </summary>
        public GamePhase CurrentPhase { get; private set; } = GamePhase.Initialization;

        /// <summary>
        /// Event fired when the game phase changes.
        /// </summary>
        public event Action<GamePhase> OnPhaseChanged;

        #endregion

        #region Turn Hooks (Stubs for Future Implementation)

        /// <summary>
        /// Event fired at the start of a turn.
        /// Systems should subscribe to perform turn-start logic.
        /// </summary>
        public event Action OnTurnStart;

        /// <summary>
        /// Event fired at the end of a turn.
        /// Systems should subscribe to perform turn-end logic.
        /// </summary>
        public event Action OnTurnEnd;

        /// <summary>
        /// Current turn number (stub - always 0 until turn system is implemented).
        /// </summary>
        public int CurrentTurn { get; private set; } = 0;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton pattern with optional persistence
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"GameStateManager: Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
                Debug.Log("GameStateManager: Set to persist across scenes.");
            }
        }

        private void OnEnable()
        {
            // Subscribe to grid ready event
            HexGridManager.OnGridReady += HandleGridReady;

            // If grid is already ready (late subscription), initialize immediately
            if (HexGridManager.Instance != null && HexGridManager.Instance.IsGridReady)
            {
                HandleGridReady(HexGridManager.Instance);
            }
        }

        private void OnDisable()
        {
            HexGridManager.OnGridReady -= HandleGridReady;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Called when the HexGridManager signals the grid is ready.
        /// </summary>
        private void HandleGridReady(HexGridManager gridManager)
        {
            if (IsMapReady) return; // Already initialized

            // Create the map service facade
            Map = new MapService(gridManager);
            IsMapReady = true;

            Debug.Log($"GameStateManager: Map ready ({Map.Width}x{Map.Height} tiles)");

            // Transition to Playing phase
            SetPhase(GamePhase.Playing);

            // Notify subscribers
            OnMapReady?.Invoke();
        }

        #endregion

        #region Phase Management

        /// <summary>
        /// Set the current game phase.
        /// </summary>
        public void SetPhase(GamePhase newPhase)
        {
            if (CurrentPhase == newPhase) return;

            GamePhase oldPhase = CurrentPhase;
            CurrentPhase = newPhase;

            Debug.Log($"GameStateManager: Phase changed from {oldPhase} to {newPhase}");
            OnPhaseChanged?.Invoke(newPhase);
        }

        /// <summary>
        /// Pause the game.
        /// </summary>
        public void Pause()
        {
            if (CurrentPhase == GamePhase.Playing)
            {
                SetPhase(GamePhase.Paused);
            }
        }

        /// <summary>
        /// Resume the game from pause.
        /// </summary>
        public void Resume()
        {
            if (CurrentPhase == GamePhase.Paused)
            {
                SetPhase(GamePhase.Playing);
            }
        }

        #endregion

        #region Turn Management (Stubs)

        /// <summary>
        /// Trigger the start of a new turn.
        /// Call this from the turn system when implemented.
        /// </summary>
        public void TriggerTurnStart()
        {
            CurrentTurn++;
            Debug.Log($"GameStateManager: Turn {CurrentTurn} started");
            OnTurnStart?.Invoke();
        }

        /// <summary>
        /// Trigger the end of the current turn.
        /// Call this from the turn system when implemented.
        /// </summary>
        public void TriggerTurnEnd()
        {
            Debug.Log($"GameStateManager: Turn {CurrentTurn} ended");
            OnTurnEnd?.Invoke();
        }

        #endregion
    }
}
