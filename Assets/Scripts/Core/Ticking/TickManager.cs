using System;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Core.Ticking
{
    /// <summary>
    /// Manages all tickable systems and processes them in priority order.
    /// Systems register/unregister themselves to receive time updates.
    /// List is kept sorted by priority to avoid per-frame allocations.
    /// Implements ITickManager for testability.
    /// </summary>
    public class TickManager : MonoBehaviour, ITickManager
    {
        public static ITickManager Instance { get; private set; }

        private readonly List<ITickable> _tickables = new List<ITickable>();
        private bool _isTicking;
        private bool _needsSort;

        // Pending operations to avoid modification during iteration
        private readonly List<ITickable> _pendingAdd = new List<ITickable>();
        private readonly List<ITickable> _pendingRemove = new List<ITickable>();

        // Reusable comparison to avoid delegate allocation
        private static readonly Comparison<ITickable> PriorityComparison =
            (a, b) => a.TickPriority.CompareTo(b.TickPriority);

        /// <summary>
        /// Number of registered tickable systems.
        /// </summary>
        public int TickableCount => _tickables.Count;

        private void Awake()
        {
            if (Instance != null && (object)Instance != this)
            {
                Debug.LogWarning($"TickManager: Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if ((object)Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Register a tickable system to receive time updates.
        /// </summary>
        /// <param name="tickable">The system to register.</param>
        public void Register(ITickable tickable)
        {
            if (tickable == null) return;

            if (_isTicking)
            {
                // Defer addition until after current tick completes
                if (!_pendingAdd.Contains(tickable) && !_tickables.Contains(tickable))
                {
                    _pendingAdd.Add(tickable);
                }
            }
            else
            {
                if (!_tickables.Contains(tickable))
                {
                    _tickables.Add(tickable);
                    _needsSort = true;
                }
            }
        }

        /// <summary>
        /// Unregister a tickable system from receiving time updates.
        /// </summary>
        /// <param name="tickable">The system to unregister.</param>
        public void Unregister(ITickable tickable)
        {
            if (tickable == null) return;

            if (_isTicking)
            {
                // Defer removal until after current tick completes
                if (!_pendingRemove.Contains(tickable))
                {
                    _pendingRemove.Add(tickable);
                }
            }
            else
            {
                _tickables.Remove(tickable);
                // No need to re-sort on removal - order is preserved
            }
        }

        /// <summary>
        /// Process all active tickable systems with the given delta time.
        /// Called by GameModeManager each frame (real-time) or per turn (turn-based).
        /// </summary>
        /// <param name="deltaTime">Time delta to pass to systems.</param>
        public void ProcessTick(float deltaTime)
        {
            if (deltaTime <= 0) return;

            // Sort if needed (only when tickables were added)
            if (_needsSort)
            {
                _tickables.Sort(PriorityComparison);
                _needsSort = false;
            }

            _isTicking = true;

            try
            {
                // Process tickables in priority order (already sorted)
                int count = _tickables.Count;
                for (int i = 0; i < count; i++)
                {
                    ITickable tickable = _tickables[i];

                    if (tickable == null || !tickable.IsTickActive)
                        continue;

                    try
                    {
                        tickable.Tick(deltaTime);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"TickManager: Error in tickable {tickable.GetType().Name}: {e.Message}");
                    }
                }
            }
            finally
            {
                _isTicking = false;
                ProcessPendingOperations();
            }
        }

        private void ProcessPendingOperations()
        {
            // Process pending additions
            if (_pendingAdd.Count > 0)
            {
                foreach (var tickable in _pendingAdd)
                {
                    if (!_tickables.Contains(tickable))
                    {
                        _tickables.Add(tickable);
                        _needsSort = true;
                    }
                }
                _pendingAdd.Clear();
            }

            // Process pending removals
            if (_pendingRemove.Count > 0)
            {
                foreach (var tickable in _pendingRemove)
                {
                    _tickables.Remove(tickable);
                }
                _pendingRemove.Clear();
            }
        }
    }
}
