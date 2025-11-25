using UnityEngine;
using System.Collections.Generic;

namespace RTS.UI
{
    /// <summary>
    /// Manages a group of tab buttons and their associated panels.
    /// Handles tab selection, hover effects, and panel switching.
    /// </summary>
    public class TabGroup : MonoBehaviour
    {
        [Header("Tab Settings")]
        [Tooltip("List of tab buttons in this group")]
        [SerializeField] private List<TabButton> tabButtons = new List<TabButton>();

        [Tooltip("Index of the tab to show on start (0-based)")]
        [SerializeField] private int defaultTabIndex = 0;

        private TabButton selectedTab;

        private void Start()
        {
            // Subscribe all tabs to this group
            foreach (TabButton tab in tabButtons)
            {
                if (tab != null)
                {
                    tab.SetTabGroup(this);
                }
            }

            // Select the default tab
            if (tabButtons.Count > 0 && defaultTabIndex >= 0 && defaultTabIndex < tabButtons.Count)
            {
                OnTabSelected(tabButtons[defaultTabIndex]);
            }
        }

        /// <summary>
        /// Called when a tab is selected/clicked.
        /// </summary>
        public void OnTabSelected(TabButton tab)
        {
            if (tab == null) return;

            selectedTab = tab;

            // Reset all tabs to idle
            ResetTabs();

            // Set selected tab to active
            tab.SetBackgroundColor(tab.GetActiveColor());

            // Show the selected panel and hide others
            ShowPanel(tab);
        }

        /// <summary>
        /// Called when mouse enters a tab.
        /// </summary>
        public void OnTabEnter(TabButton tab)
        {
            if (tab == null || tab == selectedTab) return;

            // Show hover state
            tab.SetBackgroundColor(tab.GetHoverColor());
        }

        /// <summary>
        /// Called when mouse exits a tab.
        /// </summary>
        public void OnTabExit(TabButton tab)
        {
            if (tab == null || tab == selectedTab) return;

            // Return to idle state
            tab.SetBackgroundColor(tab.GetIdleColor());
        }

        /// <summary>
        /// Resets all tabs to their idle state.
        /// </summary>
        private void ResetTabs()
        {
            foreach (TabButton tab in tabButtons)
            {
                if (tab != null && tab != selectedTab)
                {
                    tab.SetBackgroundColor(tab.GetIdleColor());
                }
            }
        }

        /// <summary>
        /// Shows the panel associated with the selected tab and hides others.
        /// </summary>
        private void ShowPanel(TabButton activeTab)
        {
            // Hide all panels first
            foreach (TabButton tab in tabButtons)
            {
                if (tab != null && tab.TargetPanel != null)
                {
                    tab.TargetPanel.SetActive(false);
                }
            }

            // Show the active panel
            if (activeTab.TargetPanel != null)
            {
                activeTab.TargetPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Manually select a tab by index.
        /// </summary>
        public void SelectTab(int index)
        {
            if (index >= 0 && index < tabButtons.Count && tabButtons[index] != null)
            {
                OnTabSelected(tabButtons[index]);
            }
        }

        /// <summary>
        /// Add a tab button to this group at runtime.
        /// </summary>
        public void AddTab(TabButton tab)
        {
            if (tab != null && !tabButtons.Contains(tab))
            {
                tabButtons.Add(tab);
                tab.SetTabGroup(this);

                // If this is the first tab, select it
                if (tabButtons.Count == 1)
                {
                    OnTabSelected(tab);
                }
            }
        }

        /// <summary>
        /// Remove a tab button from this group.
        /// </summary>
        public void RemoveTab(TabButton tab)
        {
            if (tab != null && tabButtons.Contains(tab))
            {
                tabButtons.Remove(tab);

                // If we removed the selected tab, select another
                if (selectedTab == tab && tabButtons.Count > 0)
                {
                    OnTabSelected(tabButtons[0]);
                }
            }
        }
    }
}
