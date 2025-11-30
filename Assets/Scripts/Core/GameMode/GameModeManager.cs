using UnityEngine;
using RTS.Core.Time;
using RTS.Core.Ticking;

namespace RTS.Core.GameMode
{
    /// <summary>
    /// Central manager for game mode selection and coordination.
    /// Handles initialization-time mode selection and coordinates time/tick systems.
    /// </summary>
    public class GameModeManager : MonoBehaviour
    {
        public static GameModeManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField]
        [Tooltip("Configuration for game mode settings. If null, defaults to Real-Time mode.")]
        private GameModeConfigSO modeConfig;

        private IGameMode _currentMode;
        private TimeManager _timeManager;
        private TickManager _tickManager;
        private bool _isInitialized;

        /// <summary>
        /// The currently active game mode.
        /// </summary>
        public IGameMode CurrentMode => _currentMode;

        /// <summary>
        /// Whether the current mode is Real-Time.
        /// </summary>
        public bool IsRealTime => _currentMode is RTS.RealTime.RealTimeMode;

        /// <summary>
        /// Whether the current mode is Turn-Based.
        /// </summary>
        public bool IsTurnBased => _currentMode is RTS.TurnBased.TurnBasedMode;

        /// <summary>
        /// Whether the player can currently perform actions.
        /// </summary>
        public bool CanPlayerAct => _currentMode?.CanPlayerAct ?? false;

        /// <summary>
        /// The game mode configuration.
        /// </summary>
        public GameModeConfigSO Config => modeConfig;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"GameModeManager: Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            InitializeMode();
        }

        private void OnDestroy()
        {
            if (_currentMode != null)
            {
                _currentMode.Shutdown();
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (!_isInitialized || _currentMode == null) return;

            // Let the current mode handle its per-frame logic
            _currentMode.Update();

            // Process ticks using current mode's time provider
            if (_currentMode.TimeProvider != null && !_currentMode.TimeProvider.IsPaused)
            {
                float deltaTime = _currentMode.TimeProvider.DeltaTime;
                if (deltaTime > 0 && _tickManager != null)
                {
                    _tickManager.ProcessTick(deltaTime);
                }
            }
        }

        /// <summary>
        /// Initialize the game mode based on configuration.
        /// Called automatically in Start().
        /// </summary>
        private void InitializeMode()
        {
            if (_isInitialized) return;

            // Get references to required managers
            _timeManager = TimeManager.Instance;
            _tickManager = TickManager.Instance;

            if (_timeManager == null)
            {
                Debug.LogError("GameModeManager: TimeManager.Instance is null. Ensure TimeManager exists in the scene.");
                return;
            }

            if (_tickManager == null)
            {
                Debug.LogError("GameModeManager: TickManager.Instance is null. Ensure TickManager exists in the scene.");
                return;
            }

            // Create the appropriate game mode based on config
            if (modeConfig == null)
            {
                Debug.LogWarning("GameModeManager: No config assigned. Defaulting to Real-Time mode.");
                _currentMode = new RTS.RealTime.RealTimeMode();
            }
            else
            {
                switch (modeConfig.gameMode)
                {
                    case GameModeConfigSO.GameModeType.RealTime:
                        _currentMode = new RTS.RealTime.RealTimeMode(modeConfig.initialTimeScale);
                        break;
                    case GameModeConfigSO.GameModeType.TurnBased:
                        _currentMode = new RTS.TurnBased.TurnBasedMode(modeConfig);
                        break;
                    default:
                        Debug.LogWarning($"GameModeManager: Unknown mode type {modeConfig.gameMode}. Defaulting to Real-Time.");
                        _currentMode = new RTS.RealTime.RealTimeMode();
                        break;
                }
            }

            // Initialize the mode
            _currentMode.Initialize();

            // Set the time provider
            _timeManager.SetProvider(_currentMode.TimeProvider);

            // Subscribe to game phase changes
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            }

            _isInitialized = true;
            Debug.Log($"GameModeManager: Initialized with {_currentMode.ModeName} mode.");
        }

        private void HandlePhaseChanged(GamePhase newPhase)
        {
            _currentMode?.OnPhaseChanged(newPhase);
        }

        /// <summary>
        /// Get the turn manager if in turn-based mode.
        /// Returns null if in real-time mode.
        /// </summary>
        public RTS.TurnBased.TurnManager GetTurnManager()
        {
            if (_currentMode is RTS.TurnBased.TurnBasedMode turnBasedMode)
            {
                return turnBasedMode.TurnManager;
            }
            return null;
        }
    }
}
