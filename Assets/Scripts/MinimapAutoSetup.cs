using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically finds and wires up all minimap components at runtime.
/// Attach this to any GameObject in the scene (like the MinimapPanel).
/// </summary>
public class MinimapAutoSetup : MonoBehaviour
{
    [Header("Auto-Wire (leave empty, will auto-find)")]
    [SerializeField] private bool setupOnStart = true;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupMinimap();
        }
    }
    
    [ContextMenu("Setup Minimap")]
    public void SetupMinimap()
    {
        Debug.Log("[MinimapAutoSetup] Starting automatic minimap setup...");
        
        // Find components
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[MinimapAutoSetup] Main Camera not found! Make sure your camera has the 'MainCamera' tag.");
            return;
        }
        
        Camera minimapCamera = GameObject.Find("Minimap Camera")?.GetComponent<Camera>();
        if (minimapCamera == null)
        {
            Debug.LogError("[MinimapAutoSetup] Minimap Camera not found!");
            return;
        }
        
        GameObject minimapPanelObj = GameObject.Find("MinimapPanel");
        if (minimapPanelObj == null)
        {
            Debug.LogError("[MinimapAutoSetup] MinimapPanel not found!");
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
            
            var mainCameraField = type.GetField("mainCamera", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (mainCameraField != null) mainCameraField.SetValue(controller, mainCamera);
            
            var minimapCameraField = type.GetField("minimapCamera", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (minimapCameraField != null) minimapCameraField.SetValue(controller, minimapCamera);
            
            var minimapRectField = type.GetField("minimapRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (minimapRectField != null) minimapRectField.SetValue(controller, minimapRect);
            
            var viewportIndicatorField = type.GetField("viewportIndicator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (viewportIndicatorField != null) viewportIndicatorField.SetValue(controller, viewportIndicator);
            
            Debug.Log("[MinimapAutoSetup] MinimapController configured successfully!");
        }
        
        // Setup MinimapSetup
        MinimapSetup setup = minimapCamera.GetComponent<MinimapSetup>();
        if (setup != null)
        {
            var type = typeof(MinimapSetup);
            
            var minimapCameraField = type.GetField("minimapCamera", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (minimapCameraField != null) minimapCameraField.SetValue(setup, minimapCamera);
            
            var minimapDisplayField = type.GetField("minimapDisplay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (minimapDisplayField != null) minimapDisplayField.SetValue(setup, minimapDisplay);
            
            Debug.Log("[MinimapAutoSetup] MinimapSetup configured successfully!");
            
            // Manually trigger setup
            var setupMethod = type.GetMethod("SetupMinimapRenderTexture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (setupMethod != null) setupMethod.Invoke(setup, null);
        }
        
        // Setup MinimapCamera
        MinimapCamera minimapCam = minimapCamera.GetComponent<MinimapCamera>();
        if (minimapCam != null)
        {
            var type = typeof(MinimapCamera);
            var targetField = type.GetField("targetToFollow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (targetField != null) targetField.SetValue(minimapCam, mainCamera.transform);
            
            Debug.Log("[MinimapAutoSetup] MinimapCamera configured successfully!");
        }
        
        Debug.Log("[MinimapAutoSetup] Minimap setup complete!");
    }
}