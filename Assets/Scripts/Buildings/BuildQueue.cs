using UnityEngine;
using System.Collections.Generic;
using System;
using RTS.Data;
using RTS.Buildings;
using RTS.Core.Ticking;
using RTS.Core.GameMode;
using RTS.TurnBased;

/// <summary>
/// Tracks a building in the construction queue.
/// Supports both real-time (time-based) and turn-based (turn-count) progress.
/// </summary>
[System.Serializable]
public class QueuedBuilding
{
    public Building building;
    public BuildingData data;

    // Real-time progress tracking
    public float timeRemaining;
    public float totalTime;

    // Turn-based progress tracking
    public int turnsRemaining;
    public int totalTurns;

    public QueuedBuilding(Building bldg, BuildingData buildingData)
    {
        building = bldg;
        data = buildingData;

        // Real-time values
        timeRemaining = buildingData.buildTime;
        totalTime = buildingData.buildTime;

        // Turn-based values
        turnsRemaining = buildingData.turnsToComplete;
        totalTurns = buildingData.turnsToComplete;
    }

    /// <summary>
    /// Get construction progress (0 to 1).
    /// Works for both real-time and turn-based modes.
    /// </summary>
    public float GetProgress()
    {
        // Check if we're in turn-based mode
        if (GameModeManager.Instance != null && GameModeManager.Instance.IsTurnBased)
        {
            if (totalTurns <= 0) return 1f;
            return 1f - ((float)turnsRemaining / totalTurns);
        }

        // Real-time mode
        if (totalTime <= 0) return 1f;
        return 1f - (timeRemaining / totalTime);
    }
}

/// <summary>
/// Manages the building construction queue.
/// Supports both real-time and turn-based execution modes.
/// </summary>
public class BuildQueue : MonoBehaviour, ITickable, ITurnListener
{
    public static BuildQueue Instance { get; private set; }

    [SerializeField] private int maxQueueSize = 5;

    private List<QueuedBuilding> queue = new List<QueuedBuilding>();

    public event Action<QueuedBuilding> OnBuildingStarted;
    public event Action<QueuedBuilding> OnBuildingCompleted;
    public event Action OnQueueChanged;

    public List<QueuedBuilding> Queue => queue;

    #region ITickable Implementation (Real-Time Mode)

    /// <summary>
    /// Priority for tick processing. Medium priority for construction.
    /// </summary>
    public int TickPriority => 100;

    /// <summary>
    /// Whether this tickable is active.
    /// Only active in real-time mode when queue has items.
    /// </summary>
    public bool IsTickActive
    {
        get
        {
            // Only tick in real-time mode
            if (GameModeManager.Instance != null && GameModeManager.Instance.IsTurnBased)
                return false;

            return queue.Count > 0;
        }
    }

    /// <summary>
    /// Process construction tick (real-time mode).
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last tick.</param>
    public void Tick(float deltaTime)
    {
        ProcessQueueRealTime(deltaTime);
    }

    #endregion

    #region ITurnListener Implementation (Turn-Based Mode)

    /// <summary>
    /// Called at the start of a turn.
    /// </summary>
    public void OnTurnStart(int turnNumber)
    {
        // Nothing to do at turn start for construction
    }

    /// <summary>
    /// Called at the end of a turn.
    /// Advance construction by 1 turn for all queued buildings.
    /// </summary>
    public void OnTurnEnd(int turnNumber)
    {
        ProcessQueueTurnBased();
    }

    #endregion

    #region Unity Lifecycle

    // Track whether we're using the new tick system or legacy Update
    private bool _useTickSystem = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Register with TickManager for real-time mode (if available)
        if (TickManager.Instance != null)
        {
            TickManager.Instance.Register(this);
            _useTickSystem = true;
        }

        // Register with TurnManager for turn-based mode (if available)
        RegisterWithTurnManager();
    }

    void Update()
    {
        // Fallback: If TickManager isn't available, use legacy Update-based processing
        // This ensures backward compatibility when the new managers aren't in the scene
        if (!_useTickSystem && queue.Count > 0)
        {
            // Only process in real-time mode (or when GameModeManager isn't present)
            if (GameModeManager.Instance == null || !GameModeManager.Instance.IsTurnBased)
            {
                ProcessQueueRealTime(Time.deltaTime);
            }
        }
    }

    void OnDestroy()
    {
        // Unregister from TickManager
        if (TickManager.Instance != null)
        {
            TickManager.Instance.Unregister(this);
        }

        // Unregister from TurnManager
        UnregisterFromTurnManager();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void RegisterWithTurnManager()
    {
        if (GameModeManager.Instance != null)
        {
            var turnManager = GameModeManager.Instance.GetTurnManager();
            if (turnManager != null)
            {
                turnManager.RegisterListener(this);
            }
        }
    }

    private void UnregisterFromTurnManager()
    {
        if (GameModeManager.Instance != null)
        {
            var turnManager = GameModeManager.Instance.GetTurnManager();
            if (turnManager != null)
            {
                turnManager.UnregisterListener(this);
            }
        }
    }

    #endregion

    #region Public API

    public bool AddToQueue(Building building, BuildingData data)
    {
        if (queue.Count >= maxQueueSize)
        {
            return false;
        }

        QueuedBuilding queuedBuilding = new QueuedBuilding(building, data);
        queue.Add(queuedBuilding);

        // Disable the building until construction is complete
        if (building != null)
        {
            building.StartConstruction();
        }

        OnQueueChanged?.Invoke();

        if (queue.Count == 1)
        {
            OnBuildingStarted?.Invoke(queuedBuilding);
        }
        return true;
    }

    public bool CanAddToQueue()
    {
        return queue.Count < maxQueueSize;
    }

    public void CancelBuilding(int index)
    {
        if (index < 0 || index >= queue.Count)
            return;

        QueuedBuilding canceled = queue[index];

        // Refund resources
        if (ResourceManager.Instance != null && canceled.data != null)
        {
            ResourceManager.Instance.RefundResources(canceled.data.creditsCost, canceled.data.powerRequired);
        }

        // Destroy the building object
        if (canceled.building != null)
        {
            Destroy(canceled.building.gameObject);
        }

        queue.RemoveAt(index);
        OnQueueChanged?.Invoke();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Process queue in real-time mode (continuous time).
    /// </summary>
    private void ProcessQueueRealTime(float deltaTime)
    {
        if (queue.Count == 0)
            return;

        // Process the first building in queue
        QueuedBuilding current = queue[0];

        if (current == null || current.data == null)
        {
            queue.RemoveAt(0);
            OnQueueChanged?.Invoke();
            return;
        }

        // Update construction time
        current.timeRemaining -= deltaTime;

        // Update the building's construction progress
        if (current.building != null)
        {
            current.building.UpdateConstruction(deltaTime);
        }

        // Check if construction is complete
        if (current.timeRemaining <= 0)
        {
            CompleteBuilding(current);
        }
    }

    /// <summary>
    /// Process queue in turn-based mode (discrete turns).
    /// All buildings advance by 1 turn simultaneously.
    /// </summary>
    private void ProcessQueueTurnBased()
    {
        if (queue.Count == 0)
            return;

        // Create a list of completed buildings to process after iteration
        List<QueuedBuilding> completed = new List<QueuedBuilding>();

        foreach (var queued in queue)
        {
            if (queued == null || queued.data == null)
                continue;

            // Advance by 1 turn
            queued.turnsRemaining--;

            // Update the building's visual progress
            if (queued.building != null)
            {
                float progress = queued.GetProgress();
                queued.building.UpdateConstructionProgress(progress);
            }

            // Check if construction is complete
            if (queued.turnsRemaining <= 0)
            {
                completed.Add(queued);
            }
        }

        // Complete all finished buildings
        foreach (var queued in completed)
        {
            CompleteBuilding(queued);
        }
    }

    private void CompleteBuilding(QueuedBuilding queuedBuilding)
    {
        // Enable the building
        if (queuedBuilding.building != null)
        {
            // Building is now functional
            queuedBuilding.building.gameObject.SetActive(true);
            queuedBuilding.building.CompleteConstruction();
        }

        // Remove from queue
        queue.Remove(queuedBuilding);

        OnBuildingCompleted?.Invoke(queuedBuilding);
        OnQueueChanged?.Invoke();

        // Start next building if any
        if (queue.Count > 0)
        {
            OnBuildingStarted?.Invoke(queue[0]);
        }
    }

    #endregion
}
