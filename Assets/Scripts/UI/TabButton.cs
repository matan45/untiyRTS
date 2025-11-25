using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace RTS.UI
{
    /// <summary>
    /// Individual tab button that works with TabGroup.
    /// Handles visual feedback and communicates with TabGroup.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [Tooltip("The panel this tab controls")]
        [SerializeField] private GameObject targetPanel;

        [Header("Visual Settings")]
        [Tooltip("Color when tab is idle")]
        [SerializeField] private Color idleColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        [Tooltip("Color when hovering over tab")]
        [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        [Tooltip("Color when tab is active/selected")]
        [SerializeField] private Color activeColor = Color.white;

        private TabGroup tabGroup;
        private Image background;
        private Button button;

        public GameObject TargetPanel => targetPanel;

        private void Awake()
        {
            background = GetComponent<Image>();
            button = GetComponent<Button>();

            // Add click listener
            button.onClick.AddListener(OnTabClicked);
        }

        /// <summary>
        /// Subscribe this tab to a tab group.
        /// </summary>
        public void SetTabGroup(TabGroup group)
        {
            tabGroup = group;
        }

        /// <summary>
        /// Called when tab is clicked.
        /// </summary>
        private void OnTabClicked()
        {
            tabGroup?.OnTabSelected(this);
        }

        /// <summary>
        /// Called when mouse enters the tab.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            tabGroup?.OnTabEnter(this);
        }

        /// <summary>
        /// Called when mouse exits the tab.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            tabGroup?.OnTabExit(this);
        }

        /// <summary>
        /// Set the background color of this tab.
        /// </summary>
        public void SetBackgroundColor(Color color)
        {
            if (background != null)
            {
                background.color = color;
            }
        }

        /// <summary>
        /// Get the idle color for this tab.
        /// </summary>
        public Color GetIdleColor() => idleColor;

        /// <summary>
        /// Get the hover color for this tab.
        /// </summary>
        public Color GetHoverColor() => hoverColor;

        /// <summary>
        /// Get the active color for this tab.
        /// </summary>
        public Color GetActiveColor() => activeColor;
    }
}
