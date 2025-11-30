using System;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.TurnBased
{
    /// <summary>
    /// Manages turn progression and notifies listeners of turn events.
    /// Simple implementation with just start/end events (no phases).
    /// </summary>
    public class TurnManager
    {
        private int _currentTurn;
        private bool _isTurnInProgress;
        private readonly List<ITurnListener> _listeners = new List<ITurnListener>();

        // Pending operations to avoid modification during iteration
        private readonly List<ITurnListener> _pendingAdd = new List<ITurnListener>();
        private readonly List<ITurnListener> _pendingRemove = new List<ITurnListener>();
        private bool _isNotifying;

        /// <summary>
        /// Current turn number (1-based).
        /// </summary>
        public int CurrentTurn => _currentTurn;

        /// <summary>
        /// Whether a turn is currently being processed.
        /// </summary>
        public bool IsTurnInProgress => _isTurnInProgress;

        /// <summary>
        /// Event fired when a turn starts.
        /// </summary>
        public event Action<int> OnTurnStart;

        /// <summary>
        /// Event fired when a turn ends.
        /// </summary>
        public event Action<int> OnTurnEnd;

        /// <summary>
        /// Create a new turn manager.
        /// </summary>
        public TurnManager()
        {
            _currentTurn = 0;
            _isTurnInProgress = false;
        }

        /// <summary>
        /// Register a listener to receive turn events.
        /// </summary>
        /// <param name="listener">The listener to register.</param>
        public void RegisterListener(ITurnListener listener)
        {
            if (listener == null) return;

            if (_isNotifying)
            {
                if (!_pendingAdd.Contains(listener) && !_listeners.Contains(listener))
                {
                    _pendingAdd.Add(listener);
                }
            }
            else
            {
                if (!_listeners.Contains(listener))
                {
                    _listeners.Add(listener);
                }
            }
        }

        /// <summary>
        /// Unregister a listener from receiving turn events.
        /// </summary>
        /// <param name="listener">The listener to unregister.</param>
        public void UnregisterListener(ITurnListener listener)
        {
            if (listener == null) return;

            if (_isNotifying)
            {
                if (!_pendingRemove.Contains(listener))
                {
                    _pendingRemove.Add(listener);
                }
            }
            else
            {
                _listeners.Remove(listener);
            }
        }

        /// <summary>
        /// Start a new turn.
        /// Increments turn counter and notifies all listeners.
        /// </summary>
        public void StartTurn()
        {
            if (_isTurnInProgress)
            {
                Debug.LogWarning("TurnManager: Cannot start a new turn while one is in progress.");
                return;
            }

            _currentTurn++;
            _isTurnInProgress = true;

            Debug.Log($"TurnManager: Turn {_currentTurn} started");

            // Fire event
            OnTurnStart?.Invoke(_currentTurn);

            // Notify listeners
            NotifyListeners(l => l.OnTurnStart(_currentTurn));
        }

        /// <summary>
        /// End the current turn.
        /// Notifies all listeners and processes turn-end logic.
        /// </summary>
        public void EndTurn()
        {
            if (!_isTurnInProgress)
            {
                Debug.LogWarning("TurnManager: Cannot end turn when no turn is in progress.");
                return;
            }

            Debug.Log($"TurnManager: Turn {_currentTurn} ending");

            // Fire event
            OnTurnEnd?.Invoke(_currentTurn);

            // Notify listeners
            NotifyListeners(l => l.OnTurnEnd(_currentTurn));

            _isTurnInProgress = false;

            Debug.Log($"TurnManager: Turn {_currentTurn} ended");
        }

        /// <summary>
        /// Start and immediately end a turn (for testing or auto-advance).
        /// </summary>
        public void ExecuteFullTurn()
        {
            StartTurn();
            EndTurn();
        }

        private void NotifyListeners(Action<ITurnListener> action)
        {
            _isNotifying = true;

            try
            {
                foreach (var listener in _listeners)
                {
                    if (listener != null)
                    {
                        try
                        {
                            action(listener);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"TurnManager: Error notifying listener {listener.GetType().Name}: {e.Message}");
                        }
                    }
                }
            }
            finally
            {
                _isNotifying = false;
                ProcessPendingOperations();
            }
        }

        private void ProcessPendingOperations()
        {
            foreach (var listener in _pendingAdd)
            {
                if (!_listeners.Contains(listener))
                {
                    _listeners.Add(listener);
                }
            }
            _pendingAdd.Clear();

            foreach (var listener in _pendingRemove)
            {
                _listeners.Remove(listener);
            }
            _pendingRemove.Clear();
        }
    }
}
