using UnityEngine;
using TMPro;
using System.Collections;

namespace RTS.UI
{
    /// <summary>
    /// Displays temporary UI messages to the player.
    /// Follows Single Responsibility Principle - only handles message display.
    /// </summary>
    public class UIMessageDisplay : MonoBehaviour
    {
        public static UIMessageDisplay Instance { get; private set; }

        [Header("UI References")]
        [Tooltip("Text component to display messages")]
        [SerializeField] private TextMeshProUGUI messageText;

        [Tooltip("Parent GameObject containing the message UI (for show/hide)")]
        [SerializeField] private GameObject messageContainer;

        [Header("Display Settings")]
        [Tooltip("How long to show the message before fading")]
        [SerializeField] private float displayDuration = 3f;

        [Tooltip("How long the fade out animation takes")]
        [SerializeField] private float fadeOutDuration = 0.5f;

        [Header("Message Colors")]
        [SerializeField] private Color errorColor = new Color(1f, 0.3f, 0.3f, 1f); // Red
        [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0f, 1f); // Yellow
        [SerializeField] private Color infoColor = new Color(1f, 1f, 1f, 1f); // White

        private Coroutine currentMessageCoroutine;

        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Hide message on start
            if (messageContainer != null)
            {
                messageContainer.SetActive(false);
            }
        }

        /// <summary>
        /// Shows an error message (red color).
        /// </summary>
        public void ShowError(string message)
        {
            ShowMessage(message, errorColor);
        }

        /// <summary>
        /// Shows a warning message (yellow color).
        /// </summary>
        public void ShowWarning(string message)
        {
            ShowMessage(message, warningColor);
        }

        /// <summary>
        /// Shows an info message (white color).
        /// </summary>
        public void ShowInfo(string message)
        {
            ShowMessage(message, infoColor);
        }

        /// <summary>
        /// Shows a message with a custom color.
        /// </summary>
        public void ShowMessage(string message, Color color)
        {
            if (messageText == null || messageContainer == null)
            {
                Debug.LogWarning("UIMessageDisplay: Missing UI references! Message: " + message);
                return;
            }

            // Stop any existing message
            if (currentMessageCoroutine != null)
            {
                StopCoroutine(currentMessageCoroutine);
            }

            // Set message and color
            messageText.text = message;
            messageText.color = color;

            // Show the message container
            messageContainer.SetActive(true);

            // Start fade out coroutine
            currentMessageCoroutine = StartCoroutine(HideMessageAfterDelay());
        }

        private IEnumerator HideMessageAfterDelay()
        {
            // Wait for display duration
            yield return new WaitForSeconds(displayDuration);

            // Fade out
            float elapsedTime = 0f;
            Color startColor = messageText.color;

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
                messageText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }

            // Hide container
            messageContainer.SetActive(false);

            // Reset alpha for next message
            messageText.color = new Color(startColor.r, startColor.g, startColor.b, 1f);

            currentMessageCoroutine = null;
        }

        /// <summary>
        /// Immediately hides any visible message.
        /// </summary>
        public void HideMessage()
        {
            if (currentMessageCoroutine != null)
            {
                StopCoroutine(currentMessageCoroutine);
                currentMessageCoroutine = null;
            }

            if (messageContainer != null)
            {
                messageContainer.SetActive(false);
            }

            if (messageText != null)
            {
                Color color = messageText.color;
                messageText.color = new Color(color.r, color.g, color.b, 1f);
            }
        }
    }
}
