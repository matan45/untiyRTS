using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MinimapSetup : MonoBehaviour
{
    [Header("Auto-Setup (assign in inspector first time)")]
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RawImage minimapDisplay;
    
    [Header("Render Texture Settings")]
    [SerializeField] private int textureSize = 512;
    
    private RenderTexture minimapRenderTexture;
    
    void Start()
    {
        SetupMinimapRenderTexture();
    }
    
    void OnValidate()
    {
        // Auto-setup in editor when component is added/modified
        if (!Application.isPlaying)
        {
            SetupMinimapRenderTexture();
        }
    }
    
    void SetupMinimapRenderTexture()
    {
        if (minimapCamera == null || minimapDisplay == null)
            return;
        
        // Create or recreate render texture
        if (minimapRenderTexture == null || minimapRenderTexture.width != textureSize)
        {
            if (minimapRenderTexture != null)
            {
                if (Application.isPlaying)
                    Destroy(minimapRenderTexture);
                else
                    DestroyImmediate(minimapRenderTexture);
            }
            
            minimapRenderTexture = new RenderTexture(textureSize, textureSize, 16);
            minimapRenderTexture.name = "MinimapRT";
        }
        
        // Assign to camera and UI
        minimapCamera.targetTexture = minimapRenderTexture;
        minimapDisplay.texture = minimapRenderTexture;
    }
    
    void OnDestroy()
    {
        if (minimapRenderTexture != null)
        {
            if (Application.isPlaying)
                Destroy(minimapRenderTexture);
            else
                DestroyImmediate(minimapRenderTexture);
        }
    }
}