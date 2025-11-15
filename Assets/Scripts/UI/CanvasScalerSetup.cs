using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically configures Canvas Scaler to scale with screen size
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerSetup : MonoBehaviour
{
    [Header("Reference Resolution")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);

    [Header("Match Width or Height")]
    [Range(0, 1)]
    [SerializeField] private float matchWidthOrHeight = 0.5f;

    void OnEnable()
    {
        ApplySettings();
    }

    void OnValidate()
    {
        ApplySettings();
    }

    void Awake()
    {
        ApplySettings();
    }

    void ApplySettings()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.matchWidthOrHeight = matchWidthOrHeight;

            if (Application.isPlaying)
            {
                Debug.Log($"[CanvasScalerSetup] Canvas '{gameObject.name}' configured for Scale With Screen Size mode");
            }
        }
    }
}
