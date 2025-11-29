using UnityEngine;

namespace RTS.Terrain.Data
{
    /// <summary>
    /// Configuration for player/faction colors used in territory display.
    /// Central color registry for ownership visuals.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerColorsConfig", menuName = "RTS/Terrain/Player Colors Config")]
    public class PlayerColorsConfigSO : ScriptableObject
    {
        [Header("Player Colors")]
        [Tooltip("Colors for each player (index = player ID)")]
        [SerializeField] private Color[] playerColors = new Color[]
        {
            new Color(0.2f, 0.4f, 0.8f, 1f),  // Player 0: Blue
            new Color(0.8f, 0.2f, 0.2f, 1f),  // Player 1: Red
            new Color(0.2f, 0.7f, 0.3f, 1f),  // Player 2: Green
            new Color(0.9f, 0.75f, 0.1f, 1f), // Player 3: Yellow
        };

        [Header("Fallback")]
        [Tooltip("Color used when player ID exceeds array bounds")]
        [SerializeField] private Color fallbackColor = Color.gray;

        /// <summary>
        /// Get the color for a specific player ID.
        /// Returns fallback color if player ID is out of bounds.
        /// </summary>
        /// <param name="playerId">Player ID (0-based index)</param>
        /// <returns>Player color or fallback color</returns>
        public Color GetPlayerColor(int playerId)
        {
            if (playerId < 0 || playerId >= playerColors.Length)
                return fallbackColor;
            return playerColors[playerId];
        }

        /// <summary>
        /// Number of defined player colors.
        /// </summary>
        public int PlayerCount => playerColors.Length;

        /// <summary>
        /// Check if a player ID has a defined color.
        /// </summary>
        public bool HasPlayerColor(int playerId)
        {
            return playerId >= 0 && playerId < playerColors.Length;
        }
    }
}
