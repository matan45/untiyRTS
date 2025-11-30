using UnityEngine;

namespace RTS.Core.Time
{
    /// <summary>
    /// Central time coordination singleton.
    /// Provides access to the current time provider based on game mode.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        private ITimeProvider _currentProvider;

        /// <summary>
        /// The current active time provider.
        /// </summary>
        public ITimeProvider CurrentProvider => _currentProvider;

        /// <summary>
        /// Convenience property to get delta time from current provider.
        /// Returns 0 if no provider is set.
        /// </summary>
        public float DeltaTime => _currentProvider?.DeltaTime ?? 0f;

        /// <summary>
        /// Convenience property to check if time is paused.
        /// </summary>
        public bool IsPaused => _currentProvider?.IsPaused ?? true;

        /// <summary>
        /// Convenience property for total elapsed time.
        /// </summary>
        public float TotalTime => _currentProvider?.TotalTime ?? 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"TimeManager: Duplicate instance found on {gameObject.name}. Destroying duplicate.");
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
        /// Set the current time provider.
        /// Called by GameModeManager when mode is initialized.
        /// </summary>
        /// <param name="provider">The time provider to use.</param>
        public void SetProvider(ITimeProvider provider)
        {
            _currentProvider = provider;
            Debug.Log($"TimeManager: Provider set to {provider?.GetType().Name ?? "null"}");
        }
    }
}
