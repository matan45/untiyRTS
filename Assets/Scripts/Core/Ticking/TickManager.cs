using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RTS.Core.Ticking
{
    /// <summary>
    /// Manages all tickable systems and processes them in priority order.
    /// Systems register/unregister themselves to receive time updates.
    /// </summary>
    public class TickManager : MonoBehaviour
    {
        public static TickManager Instance { get; private set; }

        private readonly List<ITickable> _tickables = new List<ITickable>();
        private bool _isTicking;

        // Pending operations to avoid modification during iteration
        private readonly List<ITickable> _pendingAdd = new List<ITickable>();
        private readonly List<ITickable> _pendingRemove = new List<ITickable>();

        /// <summary>
        /// Number of registered tickable systems.
        /// </summary>
        public int TickableCount => _tickables.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"TickManager: Duplicate instance found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
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

            _isTicking = true;

            try
            {
                // Process tickables sorted by priority (lower first)
                var activeTickables = _tickables
                    .Where(t => t != null && t.IsTickActive)
                    .OrderBy(t => t.TickPriority);

                foreach (var tickable in activeTickables)
                {
                    try
                    {
                        tickable.Tick(deltaTime);
                    }
                    catch (System.Exception e)
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
            foreach (var tickable in _pendingAdd)
            {
                if (!_tickables.Contains(tickable))
                {
                    _tickables.Add(tickable);
                }
            }
            _pendingAdd.Clear();

            // Process pending removals
            foreach (var tickable in _pendingRemove)
            {
                _tickables.Remove(tickable);
            }
            _pendingRemove.Clear();
        }
    }
}
