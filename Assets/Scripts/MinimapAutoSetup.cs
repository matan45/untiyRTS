using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically finds and wires up all minimap components at runtime.
/// Attach this to any GameObject in the scene (like the MinimapPanel).
/// </summary>
public class MinimapAutoSetup : MonoBehaviour
{
    void Start()
    {
        SetupMinimap();
    }

    [ContextMenu("Setup Minimap")]
    public void SetupMinimap()
    {
        // Find components
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Camera minimapCamera = GameObject.Find("Minimap Camera")?.GetComponent<Camera>();
        if (minimapCamera == null)
        {
            return;
        }

        GameObject minimapPanelObj = GameObject.Find("MinimapPanel");
        if (minimapPanelObj == null)
        {
            return;
        }

        RectTransform minimapRect = minimapPanelObj.GetComponent<RectTransform>();
        RawImage minimapDisplay = GameObject.Find("MinimapDisplay")?.GetComponent<RawImage>();
        RectTransform viewportIndicator = GameObject.Find("ViewportIndicator")?.GetComponent<RectTransform>();

        // Setup MinimapController
        MinimapController controller = minimapPanelObj.GetComponent<MinimapController>();
        if (controller != null)
        {
            // Use reflection to set private fields
            var type = typeof(MinimapController);

            var mainCameraField = type.GetField("mainCamera",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (mainCameraField != null) mainCameraField.SetValue(controller, mainCamera);

            var minimapCameraField = type.GetField("minimapCamera",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (minimapCameraField != null) minimapCameraField.SetValue(controller, minimapCamera);

            var minimapRectField = type.GetField("minimapRect",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (minimapRectField != null) minimapRectField.SetValue(controller, minimapRect);

            var viewportIndicatorField = type.GetField("viewportIndicator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (viewportIndicatorField != null) viewportIndicatorField.SetValue(controller, viewportIndicator);
        }

        // Setup MinimapSetup
        MinimapSetup setup = minimapCamera.GetComponent<MinimapSetup>();
        if (setup != null)
        {
            var type = typeof(MinimapSetup);

            var minimapCameraField = type.GetField("minimapCamera",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (minimapCameraField != null) minimapCameraField.SetValue(setup, minimapCamera);

            var minimapDisplayField = type.GetField("minimapDisplay",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (minimapDisplayField != null) minimapDisplayField.SetValue(setup, minimapDisplay);

            // Manually trigger setup
            var setupMethod = type.GetMethod("SetupMinimapRenderTexture",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (setupMethod != null) setupMethod.Invoke(setup, null);
        }

        // Setup MinimapCamera
        MinimapCamera minimapCam = minimapCamera.GetComponent<MinimapCamera>();
        if (minimapCam != null)
        {
            var type = typeof(MinimapCamera);
            var targetField = type.GetField("targetToFollow",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (targetField != null) targetField.SetValue(minimapCam, mainCamera.transform);
        }
    }
}