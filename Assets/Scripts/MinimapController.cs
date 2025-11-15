using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MinimapController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RectTransform minimapRect;
    [SerializeField] private RectTransform viewportIndicator;
    
    [Header("Settings")]
    [SerializeField] private float mapSize = 100f; // Total world space map size (from -50 to 50 = 100)
    [SerializeField] private float viewportScale = 0.15f; // Scale factor for viewport indicator visibility
    
    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        UpdateViewportIndicator();
    }
    
    void Update()
    {
        UpdateViewportIndicator();
    }
    
    void UpdateViewportIndicator()
    {
        if (mainCamera == null || minimapCamera == null || viewportIndicator == null || minimapRect == null)
            return;
        
        // Calculate the main camera's view frustum on the ground plane
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        // Get the four corners of the main camera's view at ground level
        Ray[] rays = new Ray[4];
        Vector3[] worldCorners = new Vector3[4];
        
        // Bottom-left, top-left, top-right, bottom-right
        rays[0] = mainCamera.ViewportPointToRay(new Vector3(0, 0, 0));
        rays[1] = mainCamera.ViewportPointToRay(new Vector3(0, 1, 0));
        rays[2] = mainCamera.ViewportPointToRay(new Vector3(1, 1, 0));
        rays[3] = mainCamera.ViewportPointToRay(new Vector3(1, 0, 0));
        
        // Calculate center and size
        Vector3 center = Vector3.zero;
        int hitCount = 0;
        
        for (int i = 0; i < 4; i++)
        {
            float enter;
            if (groundPlane.Raycast(rays[i], out enter))
            {
                worldCorners[i] = rays[i].GetPoint(enter);
                center += worldCorners[i];
                hitCount++;
            }
        }
        
        if (hitCount > 0)
        {
            center /= hitCount;
            
            // Calculate the approximate size of the viewport
            float width = Vector3.Distance(worldCorners[0], worldCorners[3]);
            float height = Vector3.Distance(worldCorners[0], worldCorners[1]);
            
            // Scale the indicator based on camera view size (with scale factor for visibility)
            float scaleX = (width / mapSize) * minimapRect.rect.width * viewportScale;
            float scaleY = (height / mapSize) * minimapRect.rect.height * viewportScale;
            viewportIndicator.sizeDelta = new Vector2(scaleX, scaleY);

            // Convert world position to minimap position
            Vector2 minimapPos = WorldToMinimapPosition(center);

            // Clamp the viewport indicator to stay within minimap bounds
            float halfWidth = scaleX / 2f;
            float halfHeight = scaleY / 2f;
            float minX = -minimapRect.rect.width / 2f + halfWidth;
            float maxX = minimapRect.rect.width / 2f - halfWidth;
            float minY = -minimapRect.rect.height / 2f + halfHeight;
            float maxY = minimapRect.rect.height / 2f - halfHeight;

            minimapPos.x = Mathf.Clamp(minimapPos.x, minX, maxX);
            minimapPos.y = Mathf.Clamp(minimapPos.y, minY, maxY);

            viewportIndicator.anchoredPosition = minimapPos;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (mainCamera == null || minimapRect == null)
            return;
        
        // Convert click position to world position
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect, eventData.position, eventData.pressEventCamera, out localPoint);
        
        Vector3 worldPosition = MinimapToWorldPosition(localPoint);
        
        // Move main camera to clicked position
        Vector3 newCameraPos = mainCamera.transform.position;
        newCameraPos.x = worldPosition.x;
        newCameraPos.z = worldPosition.z;
        mainCamera.transform.position = newCameraPos;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }
    
    private Vector2 WorldToMinimapPosition(Vector3 worldPos)
    {
        // Convert from world space (-mapSize/2 to +mapSize/2) to normalized (0 to 1)
        float normalizedX = (worldPos.x + mapSize / 2f) / mapSize;
        float normalizedZ = (worldPos.z + mapSize / 2f) / mapSize;

        // Convert normalized to minimap pixel coordinates (centered on minimap rect)
        float minimapX = (normalizedX - 0.5f) * minimapRect.rect.width;
        float minimapY = (normalizedZ - 0.5f) * minimapRect.rect.height;

        return new Vector2(minimapX, minimapY);
    }
    
    private Vector3 MinimapToWorldPosition(Vector2 minimapPos)
    {
        // Convert minimap pixel coordinates to normalized (0 to 1)
        float normalizedX = (minimapPos.x / minimapRect.rect.width) + 0.5f;
        float normalizedZ = (minimapPos.y / minimapRect.rect.height) + 0.5f;

        // Convert from normalized to world space (-mapSize/2 to +mapSize/2)
        float worldX = (normalizedX * mapSize) - (mapSize / 2f);
        float worldZ = (normalizedZ * mapSize) - (mapSize / 2f);

        return new Vector3(worldX, 0, worldZ);
    }
}