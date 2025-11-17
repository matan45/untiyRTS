using UnityEngine;

namespace RTS.Interfaces
{
    /// <summary>
    /// Interface for objects that can be selected by the player.
    /// Implements Interface Segregation Principle (ISP) - focused interface for selection behavior only.
    /// </summary>
    public interface ISelectable
    {
        /// <summary>
        /// Gets whether this object is currently selected.
        /// </summary>
        bool IsSelected { get; }

        /// <summary>
        /// Gets the GameObject associated with this selectable object.
        /// Useful for accessing transform, renderer, and other components.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// Called when this object is selected by the player.
        /// </summary>
        void Select();

        /// <summary>
        /// Called when this object is deselected by the player.
        /// </summary>
        void Deselect();
    }
}
