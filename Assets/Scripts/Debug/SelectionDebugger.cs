using UnityEngine;
using RTS.Selection;
using RTS.Actions;

/// <summary>
/// Debug utility to verify selection system setup.
/// Attach this to any GameObject to see diagnostics in the console.
/// </summary>
public class SelectionDebugger : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== SELECTION SYSTEM DIAGNOSTICS ===");

        // Check BuildingSelectionManager
        if (BuildingSelectionManager.Instance != null)
        {
            Debug.Log("✅ BuildingSelectionManager found in scene");
        }
        else
        {
            Debug.LogError("❌ BuildingSelectionManager NOT FOUND! Add it to GameManagers GameObject");
        }

        // Check BuildingActionExecutor
        if (BuildingActionExecutor.Instance != null)
        {
            Debug.Log("✅ BuildingActionExecutor found in scene");
        }
        else
        {
            Debug.LogError("❌ BuildingActionExecutor NOT FOUND! Add it to GameManagers GameObject");
        }

        Debug.Log("=== END DIAGNOSTICS ===");
    }

    private void Update()
    {
        // Debug input
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                Debug.Log($"🖱️ Clicked on: {hit.collider.gameObject.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");

                var building = hit.collider.GetComponent<RTS.Buildings.Building>();
                if (building != null)
                {
                    Debug.Log($"✅ Building component found! IsConstructed: {building.IsConstructed}");
                }
                else
                {
                    Debug.Log("❌ No Building component on clicked object");
                }
            }
            else
            {
                Debug.Log("🖱️ Clicked empty space (no raycast hit)");
            }
        }
    }
}
